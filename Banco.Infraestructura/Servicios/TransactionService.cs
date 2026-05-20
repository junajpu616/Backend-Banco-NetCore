namespace Banco.Infraestructura.Servicios;

using Banco.Aplicacion.DTOs.Transactions;
using Banco.Aplicacion.Servicios;
using Banco.Dominio.Entidades;
using Banco.Dominio.Excepciones;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _db;

    public TransactionService(ApplicationDbContext db) => _db = db;

    // ──────────────────────────────────────────────────────────────────
    // DEPÓSITO: acredita la cuenta destino. SourceAccountId = null.
    // ──────────────────────────────────────────────────────────────────
    public async Task<TransactionResponseDto> DepositAsync(Guid accountId, decimal amount, Guid executedById)
    {
        ValidateAmount(amount);

        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var account = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == accountId)
                ?? throw new BusinessException("Cuenta no encontrada.");

            account.Balance += amount;

            var tx = CreateRecord(null, accountId, amount, "Deposito", executedById);
            _db.Transactions.Add(tx);

            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            return Map(tx, null, account);
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // RETIRO: debita la cuenta origen. DestinationAccountId = null.
    // Lanza InsufficientFundsException si el saldo es menor al monto.
    // ──────────────────────────────────────────────────────────────────
    public async Task<TransactionResponseDto> WithdrawAsync(Guid accountId, decimal amount, Guid executedById)
    {
        ValidateAmount(amount);

        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var account = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == accountId)
                ?? throw new BusinessException("Cuenta no encontrada.");

            if (account.Balance < amount)
                throw new InsufficientFundsException(account.Balance, amount);

            account.Balance -= amount;

            var tx = CreateRecord(accountId, null, amount, "Retiro", executedById);
            _db.Transactions.Add(tx);

            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            return Map(tx, account, null);
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // TRANSFERENCIA ACID: debita origen y acredita destino en una sola
    // transacción de base de datos. Si el crédito falla → Rollback total.
    // ownerCheckUserId: si se pasa, verifica que la cuenta origen
    // pertenezca al usuario (regla de negocio para el rol Cliente).
    // ──────────────────────────────────────────────────────────────────
    public async Task<TransactionResponseDto> TransferAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        Guid executedById,
        Guid? ownerCheckUserId = null)
    {
        ValidateAmount(amount);

        if (sourceAccountId == destinationAccountId)
            throw new BusinessException("Las cuentas de origen y destino no pueden ser iguales.");

        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var sourceAccount = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == sourceAccountId)
                ?? throw new BusinessException("Cuenta origen no encontrada.");

            // Validación de propiedad para el rol Cliente
            if (ownerCheckUserId.HasValue && sourceAccount.UserId != ownerCheckUserId.Value)
                throw new BusinessException("No tiene permiso para transferir desde esa cuenta.");

            var destAccount = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == destinationAccountId)
                ?? throw new BusinessException("Cuenta destino no encontrada.");

            if (sourceAccount.Balance < amount)
                throw new InsufficientFundsException(sourceAccount.Balance, amount);

            // ACID Step 1: débito
            sourceAccount.Balance -= amount;

            // ACID Step 2: crédito (cualquier excepción aquí activa el catch → RollbackAsync)
            destAccount.Balance += amount;

            var tx = CreateRecord(sourceAccountId, destinationAccountId, amount, "Transferencia", executedById);
            _db.Transactions.Add(tx);

            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            return Map(tx, sourceAccount, destAccount);
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // CONSULTAS
    // ──────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<TransactionResponseDto>> GetAllAsync()
    {
        var list = await _db.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Include(t => t.ExecutedBy)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();

        return list.Select(t => Map(t, t.SourceAccount, t.DestinationAccount));
    }

    public async Task<IEnumerable<TransactionResponseDto>> GetByAccountIdAsync(
        Guid accountId,
        Guid? ownerCheckUserId = null)
    {
        // Verificar propiedad si se solicita (Cliente consultando su historial)
        if (ownerCheckUserId.HasValue)
        {
            var account = await _db.Accounts.FindAsync(accountId)
                ?? throw new BusinessException("Cuenta no encontrada.");

            if (account.UserId != ownerCheckUserId.Value)
                throw new BusinessException("No tiene acceso al historial de esa cuenta.");
        }

        var list = await _db.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Include(t => t.ExecutedBy)
            .Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();

        return list.Select(t => Map(t, t.SourceAccount, t.DestinationAccount));
    }

    // ──────────────────────────────────────────────────────────────────
    // Helpers privados
    // ──────────────────────────────────────────────────────────────────
    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessException("El monto debe ser mayor a cero.");
    }

    private static Transaction CreateRecord(
        Guid? source, Guid? dest, decimal amount, string type, Guid executedById) => new()
    {
        Id = Guid.NewGuid(),
        SourceAccountId = source,
        DestinationAccountId = dest,
        Amount = amount,
        TransactionType = type,
        ExecutedById = executedById,
        Timestamp = DateTime.UtcNow
    };

    private static TransactionResponseDto Map(Transaction t, Account? src, Account? dst) => new()
    {
        Id = t.Id,
        SourceAccountId = t.SourceAccountId,
        SourceAccountNumber = src?.AccountNumber,
        DestinationAccountId = t.DestinationAccountId,
        DestinationAccountNumber = dst?.AccountNumber,
        Amount = t.Amount,
        TransactionType = t.TransactionType,
        ExecutedById = t.ExecutedById,
        ExecutedByName = t.ExecutedBy is not null
            ? $"{t.ExecutedBy.FirstName} {t.ExecutedBy.LastName}"
            : string.Empty,
        Timestamp = t.Timestamp
    };
}

namespace Banco.Infraestructura.Datos;

using Banco.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<AccountRequest> AccountRequests { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RoleId).IsRequired(false);

            entity.HasOne(e => e.RoleDefinition)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.Property(e => e.AccountType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TransactionType).HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.SourceAccount)
                .WithMany(a => a.OutgoingTransactions)
                .HasForeignKey(e => e.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(e => e.DestinationAccount)
                .WithMany(a => a.IncomingTransactions)
                .HasForeignKey(e => e.DestinationAccountId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(e => e.ExecutedBy)
                .WithMany(u => u.ExecutedTransactions)
                .HasForeignKey(e => e.ExecutedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AccountRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.InitialBalance).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(80).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleId).IsRequired();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });
    }
}

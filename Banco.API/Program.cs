using System.Text;
using Banco.Aplicacion.CasosDeUso.Accounts;
using Banco.Aplicacion.CasosDeUso.Auth;
using Banco.Aplicacion.CasosDeUso.Users;
using Banco.Aplicacion.Repositorios;
using Banco.Aplicacion.Servicios;
using Banco.Dominio.Constantes;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Banco.Infraestructura.Repositorios;
using Banco.Infraestructura.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────────────
// Base de datos
// ──────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ──────────────────────────────────────────────────────────────────────
// Repositorios
// ──────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountRequestRepository, AccountRequestRepository>();

// ──────────────────────────────────────────────────────────────────────
// Servicios de infraestructura
// ──────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// ──────────────────────────────────────────────────────────────────────
// Casos de uso
// ──────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<CreateUserUseCase>();
builder.Services.AddScoped<GetUsersUseCase>();
builder.Services.AddScoped<CreateAccountUseCase>();
builder.Services.AddScoped<GetAccountsUseCase>();
builder.Services.AddScoped<CreateAccountRequestUseCase>();
builder.Services.AddScoped<GetAccountRequestsUseCase>();
builder.Services.AddScoped<ApproveAccountRequestUseCase>();

// ──────────────────────────────────────────────────────────────────────
// Autenticación JWT
// ──────────────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret no configurado en appsettings.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        // No extra authentication event logging in production.
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Permissions.RbacRead, policy => policy.RequireClaim("permission", Permissions.RbacRead))
    .AddPolicy(Permissions.RbacManage, policy => policy.RequireClaim("permission", Permissions.RbacManage))
    .AddPolicy(Permissions.UsersRead, policy => policy.RequireClaim("permission", Permissions.UsersRead))
    .AddPolicy(Permissions.UsersCreate, policy => policy.RequireClaim("permission", Permissions.UsersCreate))
    .AddPolicy(Permissions.AccountsRead, policy => policy.RequireClaim("permission", Permissions.AccountsRead))
    .AddPolicy(Permissions.AccountsCreate, policy => policy.RequireClaim("permission", Permissions.AccountsCreate))
    .AddPolicy(Permissions.AccountsApproveRequests, policy => policy.RequireClaim("permission", Permissions.AccountsApproveRequests))
    .AddPolicy(Permissions.TransactionsRead, policy => policy.RequireClaim("permission", Permissions.TransactionsRead))
    .AddPolicy(Permissions.TransactionsManage, policy => policy.RequireClaim("permission", Permissions.TransactionsManage))
    .AddPolicy(Permissions.RequestsCreate, policy => policy.RequireClaim("permission", Permissions.RequestsCreate))
    .AddPolicy(Permissions.RequestsRead, policy => policy.RequireClaim("permission", Permissions.RequestsRead))
    .AddPolicy(Permissions.RequestsReview, policy => policy.RequireClaim("permission", Permissions.RequestsReview));

// ──────────────────────────────────────────────────────────────────────
// Swagger con soporte Bearer
// ──────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banco Universitario API",
        Version = "v1",
        Description = "Simulador bancario universitario — Clean Architecture + RBAC + JWT"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer {tu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ──────────────────────────────────────────────────────────────────────
// CORS (para Next.js en desarrollo)
// ──────────────────────────────────────────────────────────────────────
var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:3000";

builder.Services.AddCors(opt =>
    // WARNING: Temporarily allowing any origin. Use only for short-lived testing.
    // In production, restrict origins or use configuration to whitelist domains.
    opt.AddPolicy("FrontendPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

await WaitForDatabaseAsync(app.Services);
await SeedDatabaseAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banco API v1"));

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task WaitForDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var retries = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= retries; attempt++)
    {
        if (await db.Database.CanConnectAsync())
        {
            return;
        }

        if (attempt == retries)
        {
            throw new InvalidOperationException("No se pudo conectar a la base de datos después de varios intentos.");
        }

        await Task.Delay(delay);
    }
}

static async Task SeedDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
    await EnsureRoleSchemaAsync(db);
    await EnsurePermissionSchemaAsync(db);

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    await SeedRolesAsync(db);
    await SeedUsersAsync(db, passwordHasher);
    await BackfillUserRoleIdsAsync(db);
    await SeedPermissionsAsync(db);
    await BackfillRolePermissionRoleIdsAsync(db);
    await SeedRolePermissionsAsync(db);
    // Drop legacy Role string column and its index if present (idempotent)
    await db.Database.ExecuteSqlRawAsync("""
        DROP INDEX IF EXISTS "IX_RolePermissions_Role_PermissionId";
        ALTER TABLE "RolePermissions" DROP COLUMN IF EXISTS "Role";
        """);
}

static async Task EnsureRoleSchemaAsync(ApplicationDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "Roles" (
            "Id" uuid NOT NULL,
            "Name" character varying(20) NOT NULL,
            "Description" character varying(200) NOT NULL,
            "IsActive" boolean NOT NULL DEFAULT TRUE,
            CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
        );
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Roles_Name" ON "Roles" ("Name");
        """);

    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "Users"
        ADD COLUMN IF NOT EXISTS "RoleId" uuid;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "Users"
        DROP CONSTRAINT IF EXISTS "FK_Users_Roles_RoleId";
        """);

    await db.Database.ExecuteSqlRawAsync("""
        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint
                WHERE conname = 'FK_Users_Roles_RoleId'
            ) THEN
                ALTER TABLE "Users"
                ADD CONSTRAINT "FK_Users_Roles_RoleId"
                FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE RESTRICT;
            END IF;
        END $$;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
        """);
}

static async Task EnsurePermissionSchemaAsync(ApplicationDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "Permissions" (
            "Id" uuid NOT NULL,
            "Code" character varying(80) NOT NULL,
            "Description" character varying(200) NOT NULL,
            CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
        );
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Permissions_Code" ON "Permissions" ("Code");
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS "RolePermissions" (
            "Id" uuid NOT NULL,
            "RoleId" uuid NOT NULL,
            "PermissionId" uuid NOT NULL,
            CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_RolePermissions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
        );
        """);

    // No-op: legacy `Role` string column will be dropped after seeding/backfill.
    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "RolePermissions"
        ADD COLUMN IF NOT EXISTS "RoleId" uuid;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "RolePermissions"
        ADD COLUMN IF NOT EXISTS "PermissionId" uuid;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE "RolePermissions"
        DROP CONSTRAINT IF EXISTS "FK_RolePermissions_Roles_RoleId";
        """);

    await db.Database.ExecuteSqlRawAsync("""
        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_constraint
                WHERE conname = 'FK_RolePermissions_Roles_RoleId'
            ) THEN
                ALTER TABLE "RolePermissions"
                ADD CONSTRAINT "FK_RolePermissions_Roles_RoleId"
                FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE;
            END IF;
        END $$;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_RolePermissions_RoleId_PermissionId" ON "RolePermissions" ("RoleId", "PermissionId");
        """);
}

static async Task SeedRolesAsync(ApplicationDbContext db)
{
    var seedRoles = new[]
    {
        new Role { Id = Guid.NewGuid(), Name = Roles.Admin, Description = "Rol administrador", IsActive = true },
        new Role { Id = Guid.NewGuid(), Name = Roles.Supervisor, Description = "Rol supervisor", IsActive = true },
        new Role { Id = Guid.NewGuid(), Name = Roles.Cajero, Description = "Rol cajero", IsActive = true },
        new Role { Id = Guid.NewGuid(), Name = Roles.Cliente, Description = "Rol cliente", IsActive = true }
    };

    var existing = await db.Roles.Select(role => role.Name).ToListAsync();
    var toAdd = seedRoles.Where(role => !existing.Contains(role.Name)).ToList();
    if (toAdd.Count > 0)
    {
        db.Roles.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}

static async Task SeedUsersAsync(ApplicationDbContext db, IPasswordHasher passwordHasher)
{
    if (await db.Users.AnyAsync())
    {
        return;
    }

    var roles = await db.Roles.ToDictionaryAsync(role => role.Name, role => role.Id);

    db.Users.AddRange(
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "Banco",
            Email = "admin@banco.com",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            Role = Roles.Admin,
            RoleId = roles[Roles.Admin],
            CreatedAt = DateTime.UtcNow
        },
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Supervisor",
            LastName = "Banco",
            Email = "supervisor@banco.com",
            PasswordHash = passwordHasher.Hash("Supervisor123!"),
            Role = Roles.Supervisor,
            RoleId = roles[Roles.Supervisor],
            CreatedAt = DateTime.UtcNow
        },
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Cliente",
            LastName = "Prueba",
            Email = "cliente@banco.com",
            PasswordHash = passwordHasher.Hash("Cliente123!"),
            Role = Roles.Cliente,
            RoleId = roles[Roles.Cliente],
            CreatedAt = DateTime.UtcNow
        }
    );

    await db.SaveChangesAsync();
}

static async Task BackfillUserRoleIdsAsync(ApplicationDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        UPDATE "Users" u
        SET "RoleId" = r."Id"
        FROM "Roles" r
        WHERE u."Role" = r."Name"
          AND (u."RoleId" IS NULL OR u."RoleId" <> r."Id");
        """);
}

static async Task BackfillRolePermissionRoleIdsAsync(ApplicationDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_name = 'RolePermissions'
                  AND column_name = 'Role'
            ) THEN
                UPDATE "RolePermissions" rp
                SET "RoleId" = r."Id"
                FROM "Roles" r
                WHERE rp."Role" = r."Name"
                  AND (rp."RoleId" IS NULL OR rp."RoleId" <> r."Id");
            END IF;
        END $$;
        """);
}

static async Task SeedPermissionsAsync(ApplicationDbContext db)
{
    var permissionSeeds = new[]
    {
        new Permission { Id = Guid.NewGuid(), Code = Permissions.RbacRead, Description = "Ver roles y permisos" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.RbacManage, Description = "Gestionar roles y permisos" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.UsersRead, Description = "Ver usuarios" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.UsersCreate, Description = "Crear usuarios" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.AccountsRead, Description = "Ver cuentas" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.AccountsCreate, Description = "Crear cuentas" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.AccountsApproveRequests, Description = "Aprobar solicitudes de cuenta" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.TransactionsRead, Description = "Ver transacciones" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.TransactionsManage, Description = "Gestionar transacciones" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.RequestsCreate, Description = "Crear solicitudes de cuenta" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.RequestsRead, Description = "Ver solicitudes de cuenta" },
        new Permission { Id = Guid.NewGuid(), Code = Permissions.RequestsReview, Description = "Revisar solicitudes de cuenta" }
    };

    var existingPermissions = await db.Permissions.Select(permission => permission.Code).ToListAsync();
    var permissionsToAdd = permissionSeeds.Where(permission => !existingPermissions.Contains(permission.Code)).ToList();
    if (permissionsToAdd.Count > 0)
    {
        db.Permissions.AddRange(permissionsToAdd);
        await db.SaveChangesAsync();
    }
}

static async Task SeedRolePermissionsAsync(ApplicationDbContext db)
{
    var rolesByName = await db.Roles.ToDictionaryAsync(role => role.Name, role => role.Id);
    var permissionsByCode = await db.Permissions.ToDictionaryAsync(permission => permission.Code, permission => permission.Id);

    var rolePermissionSeeds = new List<RolePermission>
    {
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.RbacRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.RbacManage] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.UsersRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.UsersCreate] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.AccountsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.AccountsCreate] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.AccountsApproveRequests] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.TransactionsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.TransactionsManage] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.RequestsCreate] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.RequestsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Admin], PermissionId = permissionsByCode[Permissions.RequestsReview] },

    new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Supervisor], PermissionId = permissionsByCode[Permissions.RbacRead] },
    new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Supervisor], PermissionId = permissionsByCode[Permissions.RbacManage] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Supervisor], PermissionId = permissionsByCode[Permissions.UsersRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Supervisor], PermissionId = permissionsByCode[Permissions.AccountsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Supervisor], PermissionId = permissionsByCode[Permissions.TransactionsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Supervisor], PermissionId = permissionsByCode[Permissions.RequestsRead] },

        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Cajero], PermissionId = permissionsByCode[Permissions.AccountsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Cajero], PermissionId = permissionsByCode[Permissions.TransactionsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Cajero], PermissionId = permissionsByCode[Permissions.TransactionsManage] },

        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Cliente], PermissionId = permissionsByCode[Permissions.AccountsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Cliente], PermissionId = permissionsByCode[Permissions.TransactionsRead] },
        new() { Id = Guid.NewGuid(), RoleId = rolesByName[Roles.Cliente], PermissionId = permissionsByCode[Permissions.RequestsCreate] }
    };

    var existingRolePermissionSet = await db.RolePermissions
        .Select(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
        .ToListAsync();

    var existingSet = existingRolePermissionSet
        .Select(rolePermission => (rolePermission.RoleId, rolePermission.PermissionId))
        .ToHashSet();

    var missing = rolePermissionSeeds
        .Where(rolePermission => !existingSet.Contains((rolePermission.RoleId, rolePermission.PermissionId)))
        .ToList();

    if (missing.Count > 0)
    {
        db.RolePermissions.AddRange(missing);
        await db.SaveChangesAsync();
    }
}

# Banco — API (Backend)

> Documentación rápida de endpoints, usos y ejemplos para el backend del simulador bancario.

## Requisitos
- .NET 8 SDK
- PostgreSQL (configurar cadena de conexión en `Banco.API/appsettings.json`)

## Configuración rápida
1. Edita `Banco.API/appsettings.json` y ajusta `ConnectionStrings:DefaultConnection` y `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`.
2. Ejecuta la API (desde la raíz del repo):

```bash
cd Banco.API
dotnet run
```

La aplicación expone Swagger en `/swagger` (por defecto `https://localhost:5001/swagger` en desarrollo).

> Nota: el proceso de arranque crea/esquema y siembra roles/usuarios de ejemplo (admin@banco.com / Admin123!, supervisor@banco.com / Supervisor123!, cliente@banco.com / Cliente123!).

---

## Autenticación

Obtener token JWT:

```bash
curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"cliente@banco.com","password":"Cliente123!"}'
```

Respuesta (ejemplo):

```json
{
  "token": "eyJhbGciOi...",
  "role": "Cliente",
  "userId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "fullName": "Cliente Prueba",
  "expiresAt": "2026-05-23T12:34:56Z"
}
```

Usar el token en llamadas autenticadas:

```
Authorization: Bearer {token}
```

---

## Endpoints principales

Todos los endpoints están bajo `/api` y documentados en Swagger. Aquí están los más usados agrupados por controlador.

### Auth
- `POST /api/auth/login` — Autentica y devuelve JWT.

Request body:

```json
{
  "email": "admin@banco.com",
  "password": "Admin123!"
}
```

---

### Users
- `GET /api/users` — (Admin, Supervisor) Lista todos los usuarios.
- `GET /api/users/{id}` — (Admin, Supervisor) Obtener usuario por id.
- `GET /api/users/me` — Devuelve perfil del usuario autenticado.
- `POST /api/users` — (Admin) Crear usuario con rol arbitrario.
- `POST /api/users/register` — (Público) Registro público que crea un usuario con rol `Cliente`.

Ejemplo de registro público:

```bash
curl -X POST http://localhost:5000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Juan","lastName":"Perez","email":"juan@example.com","password":"Pwd123!"}'
```

Payload para creación/registro (`CreateUserDto`):

```json
{
  "firstName": "Nombre",
  "lastName": "Apellido",
  "email": "correo@ejemplo.com",
  "password": "Secret123!",
  "role": "Cliente" // (solo admin puede setear roles diferentes)
}
```

---

### Accounts
- `GET /api/accounts` — (Admin, Supervisor) Lista todas las cuentas.
- `GET /api/accounts/{id}` — (Admin, Supervisor) Obtener cuenta por id.
- `GET /api/accounts/by-number/{number}` — (Autenticado) Buscar cuenta por número (útil para resolver destino por número de cuenta).
- `GET /api/accounts/my-accounts` — (Cliente) Lista cuentas propias del cliente autenticado.
- `POST /api/accounts` — (Admin) Crear cuenta para un usuario existente.

Payload para crear cuenta (`CreateAccountDto`):

```json
{
  "userId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "accountType": "Corriente",
  "initialBalance": 100.00
}
```

Ejemplo: buscar por número de cuenta

```bash
curl http://localhost:5000/api/accounts/by-number/1234567890 \
  -H "Authorization: Bearer $TOKEN"
```

Respuesta `AccountResponseDto` (ejemplo):

```json
{
  "id": "...",
  "userId": "...",
  "ownerFullName": "Cliente Prueba",
  "accountNumber": "1234567890",
  "accountType": "Corriente",
  "balance": 250.50
}
```

---

### Account Requests (solicitudes de creación)
- `POST /api/accountrequests` — (Cliente) Crear una solicitud de cuenta.
- `GET /api/accountrequests` — (Admin, permiso RequestsRead) Listar solicitudes.
- `GET /api/accountrequests/{id}` — (Admin) Obtener solicitud por id.
- `POST /api/accountrequests/{id}/approve` — (Admin, permiso AccountsApproveRequests) Aprobar solicitud.
- `POST /api/accountrequests/{id}/reject` — (Admin, permiso AccountsApproveRequests) Rechazar solicitud (body: razón opcional).

Payload `CreateAccountRequestDto` (igual que `CreateAccountDto`):

```json
{
  "userId": "...",
  "accountType": "Corriente",
  "initialBalance": 0.00
}
```

---

### Transactions
- `GET /api/transactions` — (Admin, Supervisor) Historial global de transacciones.
- `GET /api/transactions/account/{accountId}` — Historial por cuenta (el servicio valida permisos cuando corresponde).
- `POST /api/transactions/deposit` — (Admin, Supervisor, Cajero) Depósito en cuenta.
- `POST /api/transactions/withdraw` — (Admin, Supervisor, Cajero) Retiro de cuenta.
- `POST /api/transactions/transfer` — (Cliente) Transferencia desde una cuenta propia a otra cuenta.

Ejemplo payload depósito/retiro (`DepositWithdrawDto`):

```json
{
  "accountId": "...",
  "amount": 50.00
}
```

Ejemplo payload transferencia (`TransferDto`):

```json
{
  "sourceAccountId": "...",
  "destinationAccountId": "...",
  "amount": 25.00
}
```

Respuestas comunes de error:
- `400 Bad Request` con `{ message: "..." }` para validaciones y errores de negocio.
- En caso de fondos insuficientes el `400` incluye `{ message, available, requested }`.

Ejemplo curl para transferencia (Cliente):

```bash
curl -X POST http://localhost:5000/api/transactions/transfer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"sourceAccountId":"<GUID>","destinationAccountId":"<GUID>","amount":10.00}'
```

Respuesta `TransactionResponseDto` (ejemplo):

```json
{
  "id": "...",
  "sourceAccountId": "...",
  "sourceAccountNumber": "1234567890",
  "destinationAccountId": "...",
  "destinationAccountNumber": "0987654321",
  "amount": 10.00,
  "transactionType": "Transfer",
  "executedById": "...",
  "executedByName": "Cliente Prueba",
  "timestamp": "2026-05-23T12:00:00Z"
}
```

---

## Notas y buenas prácticas
- El proyecto ya incluye Swagger con soporte Bearer para probar endpoints desde UI.
- Asegúrate de cambiar `Jwt:Secret` por una cadena segura de al menos 32 caracteres.
- En desarrollo el `Program.cs` ejecuta `SeedDatabaseAsync` para crear roles y usuarios de prueba.
- Para llamadas desde el frontend en desarrollo, configura `FrontendOrigin` o usa `http://localhost:3000`.

---

Si quieres, puedo:
- Añadir ejemplos con `httpie` o Postman collection.
- Generar curl con valores reales extraídos del DB seed.
- Documentar en más detalle los códigos de error por endpoint.

---

Archivo generado automáticamente.

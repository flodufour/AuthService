# AuthService

Stateless authentication microservice for API Gateway architectures. Handles user identity, issues JWT access tokens, and manages the full refresh token lifecycle.

---

## Stack

- **ASP.NET Core 9** — Web API
- **Entity Framework Core 9** — ORM
- **MySQL** — Database (via Pomelo)
- **BCrypt** — Password hashing
- **JWT (RS256 — asymmetric)** — Access tokens
- **Scalar** — API documentation (development only)

---

## Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | Public | Create account |
| POST | `/auth/verify-email` | Public | Verify email address |
| POST | `/auth/login` | Public | Login |
| POST | `/auth/refresh` | Bearer | Rotate refresh token |
| POST | `/auth/logout` | Bearer | Revoke refresh token |
| POST | `/auth/forgot-password` | Public | Request password reset |
| POST | `/auth/reset-password` | Public | Reset password |
| GET | `/auth/me` | Bearer | Get current user profile |
| GET | `/.well-known/jwks.json` | Public | Public key set (JWKS) |

---

## Authentication Flows

### Registration
1. Password policy validation (min 10 chars, uppercase, lowercase, digit)
2. BCrypt password hashing
3. Email verification token generated (512-bit CSPRNG) → stored hashed (SHA-256)
4. Verification email sent with raw token
5. Account inactive until email is verified

### Email Verification
1. Token received → hashed → compared against database
2. Expiry checked (24h window)
3. `IsEmailVerified = true`, token cleared

### Login
1. Lookup by `NormalizedEmail` (case-insensitive)
2. `IsEmailVerified` check
3. Account lockout check (`LockedUntil`)
4. BCrypt verification
5. On failure: increment `FailedLoginAttempts` → lock for 15 min after 5 attempts
6. On success: issue JWT (RS256) + refresh token (isolated family)

### Token Refresh
1. Token validated (hash + expiry + not revoked)
2. If a revoked token is presented → entire family revoked (theft detection)
3. Rotation: old token revoked, new 512-bit token issued
4. New JWT issued

### Password Reset
1. `forgot-password`: token generated (512-bit), stored hashed, expiry 1h
2. Always returns 200 (no email enumeration)
3. `reset-password`: token + expiry validated, password policy enforced
4. Password updated, token cleared, all refresh tokens revoked (force re-login on all devices)

---

## Security

| Measure | Detail |
|---|---|
| Password hashing | BCrypt |
| JWT algorithm | RS256 (asymmetric — RSA 2048-bit) |
| JWT lifetime | 15 minutes |
| Refresh token rotation | On every renewal |
| Token theft detection | Revokes entire token family on reuse |
| Token storage | SHA-256 only — never stored in plaintext |
| Account lockout | 5 failures → 15-minute lock |
| Email verification | Required before login |
| Rate limiting | 5 req/min (login, reset) — 20 req/min (register, refresh) |
| CORS | Explicitly configured origins |
| HSTS | Enabled in production |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` |
| JWT validation | Issuer, Audience, Lifetime, Signing key |
| API documentation | Scalar — development only (`/scalar/v1`) |

---

## JWT — RS256 (Asymmetric)

AuthService uses **RS256** instead of the common symmetric HMAC-SHA256.

```
AuthService   signs with RSA private key    →  JWT
OtherService  verifies with RSA public key  →  ✅ or ❌
```

**Why this matters:** Other services only need the public key to validate tokens. Even if a consuming service is compromised, the attacker cannot forge tokens — they only have the public key, which cannot sign anything.

### JWKS Endpoint

The public key is exposed as a standard JSON Web Key Set at:

```
GET /.well-known/jwks.json
```

Other services can consume it automatically:

```csharp
// In any other ASP.NET Core service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-authservice-url";
        // Automatically fetches /.well-known/jwks.json and caches the public key
        // Re-fetches when the key rotates
    });
```

No shared secret. No manual key distribution.

### Key Rotation

To rotate the RSA key pair:

```bash
openssl genrsa -out private.pem 2048
openssl rsa -in private.pem -pubout -out public.pem
```

Update `Jwt__PrivateKey` and `Jwt__PublicKey` in production. Existing tokens signed with the old key will be rejected immediately — all active sessions will require re-login.

---

## Project Structure

```
AuthService/
├── Controllers/
│   ├── AuthController.cs
│   └── WellKnownController.cs   ← serves /.well-known/jwks.json
├── Data/
│   └── AppDbContext.cs
├── DTO/
│   ├── AuthResponse.cs
│   ├── ForgotPasswordRequest.cs
│   ├── LoginRequest.cs
│   ├── LogoutRequest.cs
│   ├── MeResponse.cs
│   ├── PasswordValidationResult.cs
│   ├── RefreshTokenRequest.cs
│   ├── RegisterRequest.cs
│   ├── ResetPasswordRequest.cs
│   └── VerifyEmailRequest.cs
├── Exceptions/
│   └── AuthException.cs
├── Interfaces/
│   ├── IAuthManager.cs
│   ├── IEmailService.cs
│   ├── IHashingService.cs
│   ├── IJwtService.cs
│   ├── IPasswordPolicyService.cs
│   ├── IRefreshTokenService.cs
│   ├── ITokenGenerator.cs
│   └── ITokenService.cs
├── Migrations/
├── Models/
│   ├── RefreshToken.cs
│   └── User.cs
├── Security/
│   ├── HashingService.cs
│   ├── JwtService.cs
│   ├── PasswordPolicyService.cs
│   └── TokenGenerator.cs
├── Services/
│   ├── AuthManager.cs
│   ├── ConsoleEmailService.cs
│   ├── RefreshTokenService.cs
│   └── TokenService.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json  ← gitignored
```

---

## Database Schema

### Users

| Column | Type | Description |
|---|---|---|
| Id | GUID | Primary key |
| Email | string | Original email (display) |
| NormalizedEmail | string UNIQUE | Uppercased email (lookups) |
| PasswordHash | string | BCrypt hash |
| IsEmailVerified | bool | Email verification status |
| CreatedAt | datetime | Account creation date |
| LastLogin | datetime? | Last successful login |
| FailedLoginAttempts | int | Consecutive failed logins |
| LockedUntil | datetime? | Lockout expiry |
| EmailVerificationToken | string? | SHA-256 hashed token |
| EmailVerificationTokenExpiry | datetime? | 24h expiry |
| PasswordResetToken | string? | SHA-256 hashed token |
| PasswordResetTokenExpiry | datetime? | 1h expiry |

### RefreshTokens

| Column | Type | Description |
|---|---|---|
| Id | GUID | Primary key |
| UserId | GUID | FK → Users |
| TokenHash | string | SHA-256 of raw token |
| FamilyId | string | Token family (rotation chain) |
| CreatedAt | datetime | Creation date |
| ExpiresAt | datetime | 7-day expiry |
| Revoked | bool | Revocation status |
| ReplacedByToken | string? | Hash of the next token |

---

## Configuration

### Development (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=...;database=authservice;user=...;password=..."
  },
  "Jwt": {
    "PrivateKey": "-----BEGIN PRIVATE KEY-----\n<key>\n-----END PRIVATE KEY-----",
    "PublicKey": "-----BEGIN PUBLIC KEY-----\n<key>\n-----END PUBLIC KEY-----",
    "Issuer": "AuthService",
    "Audience": "ApiGateway",
    "ExpiryMinutes": 15
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Production (environment variables)

```
ConnectionStrings__DefaultConnection=...
Jwt__PrivateKey=-----BEGIN PRIVATE KEY-----\nMIIE...\n-----END PRIVATE KEY-----
Jwt__PublicKey=-----BEGIN PUBLIC KEY-----\nMIIB...\n-----END PUBLIC KEY-----
Cors__AllowedOrigins__0=https://yourapp.com
```

**Why `\n` in environment variables:** Environment variables are plain strings — the shell does not interpret escape sequences. The `\n` here is a literal backslash + n. The application replaces them with real newlines before parsing the PEM key. Some platforms (Docker Compose, Kubernetes secrets) support injecting actual newlines, in which case the `\n` replacement is not needed.

Generate a new RSA key pair:
```bash
openssl genrsa -out private.pem 2048
openssl rsa -in private.pem -pubout -out public.pem
```

---

## Email Service

In development, `ConsoleEmailService` logs tokens to the console output.

For production, implement `IEmailService`:

```csharp
public class SendGridEmailService : IEmailService
{
    public Task SendPasswordResetEmailAsync(string toEmail, string token) { ... }
    public Task SendVerificationEmailAsync(string toEmail, string token) { ... }
}
```

Then swap the registration in `Program.cs`:
```csharp
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
```

---

## Running Locally

```bash
dotnet ef database update
dotnet run
```

API documentation: `http://localhost:5121/scalar/v1`

JWKS: `http://localhost:5121/.well-known/jwks.json`

# AuthService

AuthService is a stateless authentication service for microservices and API Gateway architectures.

The service handles identity, issues JWT tokens, and manages refresh token sessions while delegating all business logic to external services.

---

## System Overview

AuthService is responsible only for authentication and session management.

1. User registers with email and password  
2. Password is hashed using BCrypt  
3. Email verification token is generated and sent  
4. User verifies email  
5. User logs in with credentials  
6. JWT + refresh token are issued  
7. JWT is used for API access  
8. Refresh token is used to renew sessions  
9. Logout revokes refresh tokens  

---

## System Responsibilities

- Authentication (register / login)
- Email verification workflow
- Password reset workflow
- JWT token issuance and validation
- Refresh token lifecycle management
- Token rotation and reuse detection
- Stateless identity handling

---

## Architecture

AuthService is designed as an independent service used behind an API Gateway.

### API Gateway Responsibilities

- Request routing  
- JWT validation  
- Rate limiting  
- Centralized logging  

### AuthService Responsibilities

- Identity management  
- Token generation  
- Refresh token management  
- Email verification logic  
- Password reset logic  
- Security enforcement  

---

## Technology Stack

- ASP.NET Core Web API  
- Entity Framework Core  
- MySQL  
- JWT authentication  
- BCrypt password hashing  

---

## Software Architecture

The project follows a layered clean architecture approach.

---

### Core Layer (`Core/` equivalent: Services + Domain logic)

Contains business logic independent of infrastructure:

- Authentication logic
- Token generation logic
- Refresh token handling logic
- Email verification logic
- Password reset logic
- Session management

---

### Interfaces Layer (`Interfaces/`)

All services expose interfaces defined in a dedicated folder:

- `IAuthManager`
- `ITokenService`
- `IRefreshTokenService`
- `IHashingService`
- `IPasswordPolicyService`
- `ITokenGenerator`
- `IJwtService`
- `IEmailService`
- `IEmailVerificationService`
- `IPasswordResetService`

These interfaces allow implementations to be replaced without affecting higher-level logic.

---

### Implementation Layer (`Services/`)

Concrete implementations of interfaces:

- `AuthManager`
- `TokenService`
- `RefreshTokenService`
- `EmailVerificationService`
- `PasswordResetService`
- `EmailService`

Responsible for actual authentication workflows and token handling.

---

### Data Layer (`Data/`)

- `AppDbContext`
- Entity Framework Core configuration
- Database access logic

---

### Security Layer (`Security/`)

Handles low-level security operations:

- JWT generation and validation (`JwtService`)
- Password hashing (`HashingService`)
- Password policy enforcement (`PasswordPolicyService`)
- Token utilities (`TokenGenerator`)
- Email token hashing utilities

---

### Controllers Layer (`Controllers/`)

- `AuthController`

Handles HTTP endpoints:

- Register
- Login
- Refresh token
- Logout
- Verify email
- Resend email verification
- Request password reset
- Reset password
- Get current user (`/me`)

---

## Project Structure

<pre>AuthService/
│
├─ Controllers/
│   └─ AuthController.cs
│
├─ Interfaces/
│   ├─ IAuthManager.cs
│   ├─ ITokenService.cs
│   ├─ IRefreshTokenService.cs
│   ├─ IHashingService.cs
│   ├─ IPasswordPolicyService.cs
│   ├─ ITokenGenerator.cs
│   ├─ IJwtService.cs
│   ├─ IEmailService.cs
│   ├─ IEmailVerificationService.cs
│   └─ IPasswordResetService.cs
│
├─ Services/
│   ├─ AuthManagerService.cs
│   ├─ TokenService.cs
│   ├─ RefreshTokenService.cs
│   ├─ EmailVerificationService.cs
│   ├─ PasswordResetService.cs
│   └─ EmailService.cs
│
├─ Models/
│   ├─ User.cs
│   └─ RefreshToken.cs
│
├─ DTOs/
│   ├─ LoginRequest.cs
│   ├─ RegisterRequest.cs
│   ├─ AuthResponse.cs
│   ├─ RefreshTokenRequest.cs
│   ├─ LogoutRequest.cs
│   ├─ MeResponse.cs
│   ├─ VerifyEmailRequest.cs
│   ├─ ResendEmailVerificationRequest.cs
│   ├─ PasswordResetRequest.cs
│   ├─ ResetPasswordRequest.cs
│   └─ PasswordValidationResult.cs
│
├─ Data/
│   └─ AppDbContext.cs
│
├─ Security/
│   ├─ JwtService.cs
│   ├─ HashingService.cs
│   ├─ PasswordPolicyService.cs
│   ├─ TokenGenerator.cs
│   └─ EmailTokenHasher.cs
│
├─ Program.cs
└─ appsettings.json</pre>

---

## Authentication Flow

### Register

1. Receive email and password  
2. Validate password policy  
3. Hash password using BCrypt  
4. Create user with `IsEmailVerified = false`  
5. Generate email verification token  
6. Store hashed token in database  
7. Send verification email  

---

### Email Verification

1. User clicks verification link  
2. Token is sent to API  
3. Token is hashed and compared  
4. If valid → user is activated  
5. Token is invalidated  

---

### Login

1. Validate credentials  
2. Check if email is verified  
3. Generate JWT  
4. Generate refresh token  
5. Return authentication response  

---

### API Access

- JWT is sent in Authorization header  
- API Gateway validates token  
- Request is forwarded to services  

---

### Refresh Token

- Validate refresh token  
- Rotate refresh token  
- Revoke previous token  
- Issue new JWT  

---

### Password Reset

1. User requests password reset  
2. Reset token is generated and hashed  
3. Email is sent  
4. User submits new password + token  
5. Token is validated and password is updated  

---

### Logout

- Revoke refresh token  
- End session  

---

## Security

- BCrypt password hashing  
- Short-lived JWT (~15 minutes)  
- Refresh token rotation  
- Refresh token reuse detection  
- SHA256 hashed refresh tokens stored in DB  
- Email verification required before login  
- Password reset via secure token flow  
- HTTPS required  
- JWT key rotation support (`kid`)  

---

## JWT

- Contains user ID and claims  
- Signed using secret key  
- Short expiration time  
- Supports key rotation via `kid`  

---

## Database Schema

### User

- Id  
- Email  
- NormalizedEmail  
- PasswordHash  
- IsEmailVerified  
- CreatedAt  
- LastLogin  
- FailedLoginAttempts  
- LockedUntil  
- EmailVerificationTokenHash  
- EmailVerificationTokenExpiry  
- PasswordResetTokenHash  
- PasswordResetTokenExpiry  

---

### RefreshToken

- Id  
- TokenHash  
- UserId  
- ExpiresAt  
- CreatedAt  
- Revoked  
- ReplacedByToken  
- FamilyId  

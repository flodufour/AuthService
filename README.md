# AuthService

AuthService is a stateless authentication service for microservices and API Gateway architectures.

The service handles identity, issues JWT tokens, and manages refresh token sessions while delegating all business logic to external services.

---

## System Overview

AuthService is responsible only for authentication and session management.

1. User registers with email and password  
2. Password is hashed using BCrypt  
3. User logs in with credentials  
4. JWT + refresh token are issued  
5. JWT is used for API access  
6. Refresh token is used to renew sessions  
7. Logout revokes refresh tokens  

---

## System Responsibilities

- Authentication (register / login)
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

### Core Layer (`core/` equivalent: Services + Domain logic)

Contains business logic independent of infrastructure:
- Authentication logic
- Token generation logic
- Refresh token handling logic
- Session management

---

### Interfaces Layer (`interfaces/`)

All services expose interfaces defined in a dedicated folder:

- `IAuthManager`
- `ITokenService`
- `IRefreshTokenService`
- `IHashingService`
- `IJwtService`
- `ITokenService`
These interfaces allow implementations to be replaced without affecting higher-level logic.

---

### Implementation Layer (`Services/`)

Concrete implementations of interfaces:

- `AuthManager`
- `TokenService`
- `RefreshTokenService`

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
- Token utilities (`TokenGenerator`)

---

### Controllers Layer (`Controllers/`)

- `AuthController`
Handles HTTP endpoints:
- Register
- Login
- Refresh token
- Logout

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
│   ├─ ITokenService.cs
│   └─ IJwtService.cs
│
├─ Services/
│   │
│   ├─ AuthManagerService.cs
│   ├─ TokenService.cs
│   └─ RefreshTokenService.cs
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
│   └─ LogoutRequest.cs
│
├─ Data/
│   └─ AppDbContext.cs
│
├─ Security/
│   ├─ JwtService.cs
│   ├─ HashingService.cs
│   └─ TokenGenerator.cs
│
├─ Program.cs
└─ appsettings.json</pre>

---

## Authentication Flow

### Register

1. Receive email and password  
2. Hash password using BCrypt  
3. Store user in database  

### Login

1. Validate credentials  
2. Generate JWT  
3. Generate refresh token  
4. Return authentication response  

### API Access

- JWT is sent in Authorization header  
- API Gateway validates token  
- Request is forwarded to services  

### Refresh Token

- Validate refresh token  
- Rotate refresh token  
- Revoke previous token  
- Issue new JWT  

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
- PasswordHash  
- CreatedAt  

### RefreshToken

- TokenHash  
- UserId  
- ExpiresAt  
- CreatedAt  
- Revoked  
- ReplacedByToken  
- FamilyId  

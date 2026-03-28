# AuthService API

AuthService is a stateless authentication service for microservices and API Gateway architectures.

It handles identity, issues JWTs, and manages refresh token sessions.

---

# Core Principle

AuthService is responsible only for identity.

- Authentication and session management  
- Token issuance and validation  
- No business logic ownership  
- Other services trust JWT only  

---

# System Overview

- User registers with email + password  
- Password is hashed using BCrypt  
- Login returns JWT + refresh token  
- JWT is used for API access  
- Refresh token renews sessions  

---

# Architecture

AuthService is fully independent and can be used behind an API Gateway.

---

# Stack

- ASP.NET Core Web API  
- Entity Framework Core  
- MySQL  
- JWT authentication (stateless)  

---

# Project Structure

AuthService/
│
├─ Controllers/
│   └─ AuthController.cs
│
├─ Services/
│   ├─ AuthService.cs
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
└─ appsettings.json

---

# Auth Flow

## Register
- Create user
- Hash password (BCrypt)
- Store in DB

## Login
- Validate credentials
- Issue JWT + refresh token

## API Access
- JWT sent in Authorization header
- Services validate token

## Refresh
- Validate refresh token
- Rotate tokens
- Revoke old token

## Logout
- Revoke refresh token

---

# Security

- BCrypt password hashing  
- JWT (short-lived access tokens ~15 min)  
- Refresh token rotation  
- Token reuse detection  
- SHA256 hashed refresh tokens in DB  
- HTTPS required  

---

# JWT

- Contains user ID + claims  
- Signed with secret key  
- Short expiration time  
- Supports key rotation (kid concept)  

---

# Refresh Token Model

- TokenHash  
- UserId  
- ExpiresAt  
- CreatedAt  
- Revoked  
- ReplacedByToken  
- FamilyId  

---

# Rate Limiting

Handled at API Gateway:

- Login: strict limit (5/min)  
- General API: higher limit (100/min)  

---

# API Gateway Responsibilities

- Routing to services  
- JWT validation  
- Rate limiting  
- Centralized logging  

---

# Key Rotation

- Multiple signing keys supported  
- Tokens include key ID (kid)  
- Old keys remain valid during rotation period  

---

# Features

## Authentication
- Register / Login  
- JWT issuance  
- Refresh token system  
- Logout  

## Sessions
- Multi-device support  
- Token tracking per family  
- Revocation support  

## Security
- Password hashing  
- Token hashing  
- Stateless design  

---

# Database

## User
- Id  
- Email  
- PasswordHash  
- CreatedAt  

## RefreshToken
- TokenHash  
- UserId  
- ExpiresAt  
- CreatedAt  
- Revoked  
- ReplacedByToken  
- FamilyId  

---

# Run Locally

dotnet restore  
dotnet ef database update  
dotnet run  
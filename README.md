# Voltix – EV Dealer Operations Platform

Backend API for an Electric Vehicle (EV) Dealer Operations Management System.

## Overview
Voltix is designed to support EV dealers in managing operations including user roles, dealer accounts, inventory, and transaction workflows.

## Tech Stack
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication (Access & Refresh Token)
- Unit of Work Pattern
- Redis (Refresh Token Storage)
- Layered Architecture (Controller – Service – Repository)

## Key Features
- Authentication & Role-Based Authorization
- Dealer Management
- Inventory Management
- Secure JWT Access & Refresh Token
- Redis-based token storage
- Clean separation of business logic and data access

## Architecture
The system follows a layered architecture:
- Controllers: Handle HTTP requests
- Services: Business logic processing
- Repositories: Data access abstraction
- UnitOfWork: Transaction management

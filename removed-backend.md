# Removed Backend Functionality

This document tracks the Node.js backend functionality that has been removed in preparation for migration to C#.

## Database Files

### File: `/data.db`
SQLite database file containing:
- Employee records
- Company information
- Client data
- Time tracking reports
- Relationship tables
Will be replaced with SQL Server database in C# implementation.

## Package Dependencies

### Removed NPM Scripts
- `db:generate`: Generate database migrations with Drizzle
- `db:push`: Push schema changes to database
- `db:studio`: Open Drizzle Studio for database management
- `db:seed`: Seed database with initial data
- `db:add-reports`: Add sample report data
- `db:migrate`: Apply database migrations
- `db:update-client`: Update client column structure

### Removed Dependencies
- `better-sqlite3`: SQLite database driver
- `csv-parse`: CSV file parsing
- `drizzle-kit`: Database migration toolkit
- `drizzle-orm`: ORM for database operations
- `svix`: Webhook handling library

### Removed Dev Dependencies
- `@types/better-sqlite3`: TypeScript types for SQLite
- `ts-node`: TypeScript execution environment
- `tsx`: TypeScript execution tool

## Authentication Middleware

### File: `/middleware.ts`
Authentication middleware configuration:
- Removed API route exclusions for:
  - `/api/webhook`
  - `/api/auth/registration-status`
  - `/api/auth/set-admin-role`
  - `/api/auth/first-user`
- Kept public route access for:
  - Login pages
  - Signup pages
  - Password reset pages
- All other routes require authentication

## Database Maintenance Scripts

### File: `/clean_duplicates.py`
Python script for cleaning duplicate client records:
- Connects to SQLite database
- Identifies duplicate records in clients table
- Handles deduplication based on:
  - Primary key if available
  - All columns if no primary key
- Maintains data integrity using transactions
- Provides detailed logging of:
  - Initial record count
  - Removed duplicates
  - Final record count
- Features:
  - Table structure validation
  - Error handling with rollback
  - Progress reporting
  - Safe duplicate removal

## Database Layer

### File: `/db/index.ts`
Main database configuration:
- Uses SQLite with Better-SQLite3 driver
- Drizzle ORM integration
- Handles database connection and migrations

### File: `/db/schema.ts`
Database schema definition using Drizzle ORM:

#### Tables
1. `employees`
   - Employee information
   - Clerk authentication integration
   - Department and position tracking

2. `companies`
   - Company details
   - Contact information
   - Project tracking

3. `clients`
   - Client information

4. `reports`
   - Time tracking reports
   - Links to employees and clients
   - Project and job details

5. `reportsToCompanies`
   - Many-to-many relationship between reports and companies

#### Relations
- Employee to Reports (one-to-many)
- Companies to Reports (many-to-many)
- Reports to Clients (many-to-one)

### File: `/db/queries.ts`
Database query implementations:

#### Employee Queries
- Get all employees
- Get employee by ID
- Get employee by Clerk ID
- Add new employee
- Update employee
- Delete employee

#### Company Queries
- Get all companies
- Get company by ID
- Add new company
- Update company
- Delete company

#### Client Queries
- Get all clients
- Get client by ID
- Add new client
- Update client
- Delete client

#### Report Queries
- Get all reports
- Get reports with employee information
- Get reports by employee ID
- Get detailed report by ID
- Add new report with company associations
- Update report and company associations
- Delete report

### Additional Database Files
- `init.ts`: Database initialization
- `migrations.ts`: Database migration handling
- `seed.ts`: Initial data seeding
- `check-employees.ts`: Employee validation
- `load-reports.ts`: Report data loading
- `update-company-data.ts`: Company data updates
- `update-data.ts`: General data updates
- `update-employees.ts`: Employee data updates

### File: `/drizzle.config.ts`
Drizzle ORM configuration:
- Migration settings
- Database connection configuration
- Schema compilation options
- SQLite-specific settings

## Database Scripts

### File: `/scripts/add-sample-reports.js`
Script for generating sample report data:
- Creates 10 sample reports for accounting department
- Generates random dates within last 2 months
- Associates reports with random companies
- Includes various job types and projects
- Uses transaction for data consistency

### File: `/scripts/apply-migration.ts`
Database migration script:
- Applies all pending migrations
- Uses Drizzle ORM migrator
- Reads migrations from `./drizzle` directory

### File: `/scripts/update-client-column.ts`
Client data migration script:
- Updates reports table structure
- Converts client column from text to foreign key
- Preserves existing client relationships
- Handles data migration with temporary tables
- Maintains referential integrity

## Database Migrations

### Directory: `/drizzle`
Contains all database migration files and metadata:

#### Migration Files
1. `0000_unique_outlaw_kid.sql`
   - Initial database schema setup
   - Creates base tables structure

2. `0001_nappy_invisible_woman.sql`
   - Adds authentication-related fields
   - Updates employee table structure

3. `0002_gorgeous_meggan.sql`
   - Adds company relationship tables
   - Enhances report tracking capabilities

4. `0003_foamy_tattoo.sql`
   - Adds additional fields to reports
   - Improves data tracking capabilities

5. `0004_clients_foreign_key.sql`
   - Converts client references to foreign keys
   - Improves data integrity

#### Migration Metadata
- `meta/_journal.json`: Migration history tracking
- `meta/0000_snapshot.json`: Initial schema snapshot
- `meta/0001_snapshot.json`: Auth updates snapshot
- `meta/0002_snapshot.json`: Company relations snapshot
- `meta/0003_snapshot.json`: Report updates snapshot

## Admin API Routes

### File: `/app/api/admin/route.ts`

This file contains the main admin API endpoints that handle administrative operations:

#### GET Endpoint
- Fetches dashboard statistics
- Returns counts of employees and companies
- Uses database queries from `employeeQueries` and `companyQueries`

#### POST Endpoint
Multiple actions handled through a single POST endpoint:

1. `getEmployees`
   - Retrieves all employees
   - Enhances employee data with admin role information from Clerk
   - Returns list of employees with their admin status

2. `addEmployee`
   - Creates new employee accounts
   - Creates user in Clerk authentication system
   - Adds employee to database
   - Handles password and email setup
   - Requires admin authorization

3. `updateEmployee`
   - Updates employee information
   - Updates both database and Clerk user data
   - Handles password changes
   - Manages admin role assignments
   - Requires admin authorization

4. `deleteEmployee`
   - Removes employee from database
   - Deletes associated Clerk user account
   - Requires admin authorization

5. `getCompanies`
   - Retrieves all companies
   - Calculates company statistics including:
     - Active projects
     - Total hours
     - Revenue calculations
   - Combines data from companies and reports tables

Dependencies:
- Clerk Authentication (`@clerk/nextjs/server`)
- Next.js Response handling
- Database queries from local DB layer

### File: `/app/api/auth/set-admin-role/route.ts`

Endpoint for managing admin roles:

#### POST Endpoint
- Sets a user as admin by their user ID
- Updates Clerk user metadata
- Requires existing user ID

### File: `/app/api/auth/first-user/route.ts`

Handles first user registration and admin assignment:

#### GET Endpoint
- Checks if current user is the first in the system
- Assigns admin rights to first user
- Creates employee record for first admin
- Manages concurrent access with locking mechanism
- Integrates with Clerk authentication

### File: `/app/api/auth/registration-status/route.ts`

Controls user registration availability:

#### GET Endpoint
- Checks if any users exist in local database
- Checks if any users exist in Clerk
- Determines if new registrations should be allowed
- Manages registration status for first admin user

### File: `/app/api/employee/current/route.ts`

Handles current employee information:

#### GET Endpoint
- Retrieves current employee details based on Clerk session
- Maps Clerk user to employee record
- Returns employee data or 404 if not found

### File: `/app/api/me/route.ts`

Handles user profile information:

#### GET Endpoint
- Retrieves current user data from Clerk
- Returns formatted user profile information
- Includes user metadata and roles

### File: `/app/api/webhook/route.ts`

Handles Clerk authentication webhooks:

#### POST Endpoint
- Processes Clerk webhook events
- Handles user creation events
- Sets up first admin user
- Manages employee records
- Validates webhook signatures

### File: `/app/api/reports/route.ts`

Handles report management:

#### GET Endpoint
- Retrieves reports with filtering options
- Supports current user filtering
- Includes employee and company data

#### POST Endpoint
- Creates new reports
- Associates reports with employees
- Handles company relationships

#### DELETE Endpoint
- Removes reports
- Validates user permissions
- Ensures users can only delete their own reports

### File: `/app/api/clients/route.ts`

Handles client data access:

#### GET Endpoint
- Retrieves all clients from database
- Requires user authentication
- Returns list of all clients
- Protected endpoint requiring valid session 
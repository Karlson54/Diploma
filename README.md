# Diploma Project - Database Setup with Drizzle ORM

This project uses Drizzle ORM with SQLite to manage the database. Below are instructions on how to set up and use the database.

## Database Structure

The database includes the following tables:

1. **Employees** - Stores information about employees
   - id (primary key)
   - name
   - email
   - position
   - department
   - joinDate
   - status

2. **Companies** - Stores information about companies
   - id (primary key)
   - name
   - contact
   - email
   - phone
   - projects
   - address
   - notes

3. **Clients** - Stores a list of client names
   - id (primary key)
   - name

4. **Reports** - Stores time tracking reports
   - id (primary key)
   - employeeId (foreign key to employees)
   - date
   - market
   - contractingAgency
   - client
   - projectBrand
   - media
   - jobType
   - comments
   - hours

5. **Reports to Companies** - Junction table linking reports and companies (many-to-many)
   - id (primary key)
   - reportId (foreign key to reports)
   - companyId (foreign key to companies)

## Getting Started

### Prerequisites

- Node.js
- npm

### Installation

1. Clone the repository
2. Install dependencies:
   ```
   npm install
   ```

### Database Setup

1. Generate database migrations based on the schema:
   ```
   npm run db:generate
   ```

2. Initialize the database:
   ```
   npm run db:init
   ```

3. Seed the database with initial data:
   ```
   npm run db:seed
   ```

4. (Optional) View and modify the database using Drizzle Studio:
   ```
   npm run db:studio
   ```

## Database Usage in Code

To use the database in your components or API routes, import the query functions from the `db/queries.ts` file:

```typescript
import { employeeQueries, companyQueries, clientQueries, reportQueries } from '../db/queries';

// Example: Get all employees
const employees = await employeeQueries.getAll();

// Example: Add a new employee
const newEmployee = await employeeQueries.add({
  name: "John Doe",
  email: "john@example.com",
  position: "Developer",
  department: "Engineering",
  joinDate: "01.05.2023",
  status: "Active"
});

// Example: Get reports with employee information
const reports = await reportQueries.getAllWithEmployee();
```

## Available Scripts

- `npm run dev` - Start the development server
- `npm run build` - Build the project for production
- `npm run start` - Start the production server
- `npm run lint` - Run ESLint to check code quality
- `npm run db:generate` - Generate database migrations
- `npm run db:push` - Push schema changes to the database
- `npm run db:studio` - Open Drizzle Studio to view and modify the database
- `npm run db:init` - Initialize the database
- `npm run db:seed` - Seed the database with initial data
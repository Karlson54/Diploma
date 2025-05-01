import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import { db } from './index';

export async function addCompanyColumn() {
  try {
    // Add the company column
    db.run(`ALTER TABLE employees ADD COLUMN company TEXT;`);
    
    // Update existing employees to set company equal to department
    db.run(`UPDATE employees SET company = department;`);
    
    return true;
  } catch (error) {
    console.error('Error in migration:', error);
    return false;
  }
}

export async function renameCompanyToAgency() {
  try {
    // Add the agency column
    db.run(`ALTER TABLE employees ADD COLUMN agency TEXT;`);
    
    // Copy data from company to agency
    db.run(`UPDATE employees SET agency = company;`);
    
    return true;
  } catch (error) {
    console.error('Error in migration:', error);
    return false;
  }
}

export async function removeCompanyColumn() {
  try {
    // Create new table without company column
    db.run(`
      CREATE TABLE employees_new (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL,
        email TEXT,
        department TEXT,
        agency TEXT,
        clerkId TEXT,
        role TEXT DEFAULT 'user',
        active INTEGER DEFAULT 1
      );
    `);
    
    // Migrate data to new structure
    db.run(`
      INSERT INTO employees_new (id, name, email, department, agency, clerkId, role, active)
      SELECT id, name, email, department, agency, clerkId, role, active FROM employees;
    `);
    
    // Remove old table structure
    db.run(`DROP TABLE employees;`);
    
    // Rename new table to employees
    db.run(`ALTER TABLE employees_new RENAME TO employees;`);
    
    return true;
  } catch (error) {
    console.error('Error in migration:', error);
    return false;
  }
}

// Run the migration
removeCompanyColumn();

// Previous migrations for reference
// async function migrateAddCompanyColumn() {
//   console.log('🔄 Starting database migration to add company column...');
//   
//   try {
//     // Check if SQLite supports ADD COLUMN operation
//     const pragmaResult = await db.get("PRAGMA table_info(employees)");
//     
//     // If we got here, the database is ready for migration
//     
//     // Add company column to employees table if it doesn't exist
//     console.log('📊 Adding "company" column to employees table...');
//     await db.run(`ALTER TABLE employees ADD COLUMN company TEXT;`);
//     
//     // Update employees to set company equal to department
//     console.log('📊 Updating employees to set company = department...');
//     await db.run(`UPDATE employees SET company = department WHERE company IS NULL;`);
//     
//     console.log('✅ Migration completed successfully!');
//   } catch (error) {
//     console.error('❌ Migration failed:', error);
//   }
// }
// 
// // Run the migration
// migrateAddCompanyColumn(); 
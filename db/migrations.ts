import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import { db } from './index';

async function migrateAddCompanyColumn() {
  console.log('🔄 Starting database migration to add company column...');
  
  // Initialize the database connection
  const sqlite = new Database('./data.db');
  const db = drizzle(sqlite);
  
  try {
    // Add company column to employees table if it doesn't exist
    console.log('📊 Adding "company" column to employees table...');
    await db.run(`ALTER TABLE employees ADD COLUMN company TEXT;`);
    
    // Update employees to set company equal to department
    console.log('📊 Updating employees to set company = department...');
    await db.run(`UPDATE employees SET company = department WHERE company IS NULL;`);
    
    console.log(`\n✅ Migration completed successfully!`);
    
  } catch (error) {
    console.error('❌ Migration failed:', error);
  } finally {
    sqlite.close();
  }
}

async function migrateCompanyToAgency() {
  console.log('🔄 Starting database migration to rename company column to agency...');
  
  try {
    // Create a new agency column
    console.log('📊 Adding "agency" column to employees table...');
    await db.run(`ALTER TABLE employees ADD COLUMN agency TEXT;`);
    
    // Copy data from company column to agency column
    console.log('📊 Copying data from company column to agency column...');
    await db.run(`UPDATE employees SET agency = company;`);
    
    // You can't drop columns in SQLite directly, but in a production environment 
    // you would recreate the table without the company column
    console.log('✅ Migration completed successfully!');
  } catch (error) {
    console.error('❌ Migration failed:', error);
  }
}

async function removeCompanyColumn() {
  console.log('🔄 Starting migration to remove company column...');
  
  // Initialize a direct connection to SQLite
  const sqlite = new Database('./data.db');
  
  try {
    // In SQLite, you can't directly drop a column.
    // We need to create a new table without the column, copy the data, and swap tables.
    
    // Step 1: Begin transaction
    sqlite.prepare('BEGIN TRANSACTION').run();
    
    // Step 2: Create a new table without the company column
    console.log('📊 Creating new employees table without company column...');
    sqlite.prepare(`
      CREATE TABLE employees_new (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL,
        email TEXT NOT NULL,
        position TEXT NOT NULL,
        department TEXT NOT NULL,
        join_date TEXT NOT NULL,
        status TEXT NOT NULL,
        clerk_id TEXT,
        agency TEXT
      )
    `).run();
    
    // Step 3: Copy data from old table to new table
    console.log('📊 Migrating data to new structure...');
    sqlite.prepare(`
      INSERT INTO employees_new (id, name, email, position, department, join_date, status, clerk_id, agency)
      SELECT id, name, email, position, department, join_date, status, clerk_id, agency FROM employees
    `).run();
    
    // Step 4: Drop the old table
    console.log('📊 Removing old table structure...');
    sqlite.prepare('DROP TABLE employees').run();
    
    // Step 5: Rename the new table to the original name
    console.log('📊 Finalizing new structure...');
    sqlite.prepare('ALTER TABLE employees_new RENAME TO employees').run();
    
    // Step 6: Commit transaction
    sqlite.prepare('COMMIT').run();
    
    console.log('✅ Migration completed successfully! Column "company" has been removed.');
  } catch (error) {
    // Rollback on error
    console.error('❌ Migration failed:', error);
    sqlite.prepare('ROLLBACK').run();
  } finally {
    sqlite.close();
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
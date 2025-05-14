import { db } from '../db/index';
import { migrate } from 'drizzle-orm/better-sqlite3/migrator';
import fs from 'fs';
import path from 'path';

// Read and execute the migration
async function applyMigration() {
  console.log('🔄 Applying database migrations...');
  
  try {
    // Apply all migrations using drizzle migrator
    migrate(db, { migrationsFolder: './drizzle' });
    
    console.log('✅ Migrations applied successfully!');
  } catch (error) {
    console.error('❌ Migration failed:', error);
  }
  // db.close() не нужен для drizzle-orm/better-sqlite3, 
  // соединение управляется автоматически
}

// Run the migration
applyMigration(); 
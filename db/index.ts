import { drizzle } from 'drizzle-orm/better-sqlite3';
import { migrate } from 'drizzle-orm/better-sqlite3/migrator';
import Database from 'better-sqlite3';
import * as schema from './schema';
import path from 'path';

// Initialize database connection
const sqlite = new Database(path.resolve(process.cwd(), 'data.db'));
export const db = drizzle(sqlite, { schema });

// Run migrations on database initialization
export async function runMigrations() {
  try {
    migrate(db, { migrationsFolder: path.resolve(process.cwd(), 'drizzle') });
  } catch (error) {
    console.error('Error running migrations:', error);
    throw error;
  }
} 
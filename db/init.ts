import { runMigrations } from './index';

/**
 * Initialize the database
 * This script runs the migrations to create the database schema
 */
async function init() {
  try {
    await runMigrations();
    console.log('Database initialized successfully!');
  } catch (error) {
    console.error('Failed to initialize database:', error);
    process.exit(1);
  }
}

init(); 
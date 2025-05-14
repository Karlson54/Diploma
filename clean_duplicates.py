import sqlite3
import os

# Verify the database file exists
if not os.path.exists('data.db'):
    print("Database file 'data.db' not found.")
    exit(1)

# Connect to the database
conn = sqlite3.connect('data.db')
cursor = conn.cursor()

# List all tables in the database
cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = cursor.fetchall()
print("Tables in database:", [table[0] for table in tables])

# Check if 'clients' table exists
if ('clients',) not in tables:
    print("The 'clients' table does not exist in the database.")
    conn.close()
    exit(1)

# Get column information for the clients table
cursor.execute("PRAGMA table_info(clients)")
columns = cursor.fetchall()
print("\nColumns in 'clients' table:")
for col in columns:
    print(f"- {col[1]} ({col[2]})")

# Find duplicate records
print("\nFinding duplicates in 'clients' table...")
column_names = [col[1] for col in columns]
unique_identifier = None

# Try to find a primary key or unique identifier
for col in columns:
    if col[5] == 1:  # This column is a primary key
        unique_identifier = col[1]
        break

if unique_identifier:
    print(f"Primary key found: {unique_identifier}")
else:
    print("No primary key found. Will use all columns to identify duplicates.")

# Count total records before cleaning
cursor.execute("SELECT COUNT(*) FROM clients")
initial_count = cursor.fetchone()[0]
print(f"\nInitial record count: {initial_count}")

# Create a temporary table to store unique records
cursor.execute("BEGIN TRANSACTION")
try:
    if unique_identifier:
        # If we have a primary key, use it to identify unique records
        # We'll keep the record with the lowest primary key value
        print(f"Removing duplicates based on all fields except {unique_identifier}...")
        
        # Create a list of columns excluding the primary key for grouping
        non_pk_columns = [col for col in column_names if col != unique_identifier]
        
        # Prepare column list for the query
        columns_list = ", ".join(non_pk_columns)
        
        # Delete duplicates keeping the record with the minimum ID
        delete_query = f"""
        DELETE FROM clients
        WHERE {unique_identifier} NOT IN (
            SELECT MIN({unique_identifier})
            FROM clients
            GROUP BY {columns_list}
        )
        """
        cursor.execute(delete_query)
    else:
        # If no primary key, create a temporary table with unique records
        all_columns = ", ".join(column_names)
        
        # Create temporary table with unique records
        cursor.execute(f"CREATE TEMPORARY TABLE clients_unique AS SELECT DISTINCT {all_columns} FROM clients")
        
        # Delete all records from the original table
        cursor.execute("DELETE FROM clients")
        
        # Insert unique records back into the original table
        cursor.execute(f"INSERT INTO clients SELECT {all_columns} FROM clients_unique")
        
        # Drop the temporary table
        cursor.execute("DROP TABLE clients_unique")
    
    # Commit the changes
    conn.commit()
    
    # Count records after cleaning
    cursor.execute("SELECT COUNT(*) FROM clients")
    final_count = cursor.fetchone()[0]
    print(f"Final record count: {final_count}")
    print(f"Removed {initial_count - final_count} duplicate records.")
    
except Exception as e:
    conn.rollback()
    print(f"Error while removing duplicates: {e}")
finally:
    conn.close()

print("\nDuplicate removal process completed.") 
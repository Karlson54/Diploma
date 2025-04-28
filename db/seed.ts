import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import fs from 'fs';
import { parse } from 'csv-parse/sync';
import { clients, companies, employees } from './schema';

// Initialize the database
const sqlite = new Database('./data.db');
const db = drizzle(sqlite);

// Skip migrations as tables already exist
// migrate(db, { migrationsFolder: './drizzle' });

// Create a clean version of email from name
function createEmail(name: string, agency: string): string {
  const nameParts = name.toLowerCase().split(' ');
  const firstName = nameParts[0] || '';
  const lastName = nameParts.length > 1 ? nameParts[1] : '';
  
  // Remove non-alphanumeric characters
  const cleanFirstName = firstName.replace(/[^a-z0-9]/g, '');
  const cleanLastName = lastName.replace(/[^a-z0-9]/g, '');
  const cleanAgency = agency.toLowerCase().replace(/[^a-z0-9]/g, '');
  
  return `${cleanFirstName}${cleanLastName ? '.' + cleanLastName : ''}@${cleanAgency || 'company'}.com`;
}

async function seed() {
  console.log('🌱 Seeding database...');
  
  try {
    // Read and parse the CSV file
    const csvData = fs.readFileSync('./Книга1.csv', 'utf8');
    const records = parse(csvData, {
      skip_empty_lines: true,
      trim: true,
    });
    
    // Extract data from CSV
    const agencies = new Set<string>();
    const peopleData: { name: string; agency: string }[] = [];
    const clientsData = new Set<string>();
    const contractingAgencies = new Set<string>();
    const mediaTypes = new Set<string>();
    const jobTypes = new Set<string>();
    
    console.log(`📊 Processing ${records.length} CSV records...`);
    
    // Skip the first two rows (headers)
    for (let i = 2; i < records.length; i++) {
      const record = records[i];
      
      // Agency (Column B)
      if (record[1] && record[1].trim()) {
        agencies.add(record[1].trim());
      }
      
      // People (Column C)
      if (record[2] && record[2].trim()) {
        peopleData.push({
          name: record[2].trim(),
          agency: record[1] ? record[1].trim() : 'Unknown',
        });
      }
      
      // Media types (Column D)
      if (record[3] && record[3].trim()) {
        mediaTypes.add(record[3].trim());
      }
      
      // Job types (Column E)
      if (record[4] && record[4].trim()) {
        jobTypes.add(record[4].trim());
      }
      
      // Clients (Column F)
      if (record[5] && record[5].trim()) {
        clientsData.add(record[5].trim());
      }
      
      // Contracting Agency (Column G)
      if (record[6] && record[6].trim()) {
        contractingAgencies.add(record[6].trim());
      }
    }
    
    // Insert agencies as companies
    console.log('🏢 Inserting agencies as companies...');
    let agencyCount = 0;
    for (const agency of agencies) {
      if (agency) {
        try {
          await db.insert(companies).values({
            name: agency,
            contact: 'N/A',
            email: `contact@${agency.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')}.com`,
            phone: 'N/A',
            projects: 0,
            address: 'N/A',
            notes: 'Agency imported from CSV',
          }).onConflictDoNothing();
          agencyCount++;
        } catch (error) {
          console.error(`Failed to insert agency "${agency}":`, error);
        }
      }
    }
    
    // Insert contracting agencies as companies if not already added
    console.log('🏢 Inserting contracting agencies as companies...');
    let contractingAgencyCount = 0;
    for (const agency of contractingAgencies) {
      if (agency && !agencies.has(agency)) {
        try {
          await db.insert(companies).values({
            name: agency,
            contact: 'N/A',
            email: `contact@${agency.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')}.com`,
            phone: 'N/A',
            projects: 0,
            address: 'N/A',
            notes: 'Contracting Agency imported from CSV',
          }).onConflictDoNothing();
          contractingAgencyCount++;
        } catch (error) {
          console.error(`Failed to insert contracting agency "${agency}":`, error);
        }
      }
    }
    
    // Insert people as employees
    console.log('👥 Inserting people as employees...');
    let employeeCount = 0;
    for (const person of peopleData) {
      if (person.name) {
        try {
          await db.insert(employees).values({
            name: person.name,
            email: createEmail(person.name, person.agency),
            position: 'Specialist',
            department: person.agency,
            joinDate: new Date().toISOString().split('T')[0],
            status: 'Active',
            company: person.agency,
          }).onConflictDoNothing();
          employeeCount++;
        } catch (error) {
          console.error(`Failed to insert employee "${person.name}":`, error);
        }
      }
    }
    
    // Insert clients
    console.log('🔶 Inserting clients...');
    let clientCount = 0;
    for (const client of clientsData) {
      if (client) {
        try {
          await db.insert(clients).values({
            name: client,
          }).onConflictDoNothing();
          clientCount++;
        } catch (error) {
          console.error(`Failed to insert client "${client}":`, error);
        }
      }
    }
    
    console.log('\n✅ Seed completed successfully!');
    console.log(`📊 Summary:`);
    console.log(`  - Inserted ${agencyCount} agencies as companies`);
    console.log(`  - Inserted ${contractingAgencyCount} contracting agencies`);
    console.log(`  - Inserted ${employeeCount} employees`);
    console.log(`  - Inserted ${clientCount} clients`);
    console.log(`  - Found ${mediaTypes.size} media types`);
    console.log(`  - Found ${jobTypes.size} job types`);
    
  } catch (error) {
    console.error('❌ Seed failed:', error);
  } finally {
    sqlite.close();
  }
}

seed(); 
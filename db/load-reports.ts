import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import { reports, reportsToCompanies, employees, companies } from './schema';

// Initialize the database
const sqlite = new Database('./data.db');
const db = drizzle(sqlite);

async function generateRandomDate(start: Date, end: Date): Promise<string> {
  const date = new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
  return date.toISOString().split('T')[0]; // Format as YYYY-MM-DD
}

async function loadReports() {
  console.log('🌱 Loading 10 more reports into database...');
  
  try {
    // Get all employees
    const allEmployees = await db.select().from(employees).all();
    
    if (allEmployees.length === 0) {
      console.log('❌ No employees found. Please add employees before creating reports.');
      return;
    }
    
    // Get all companies
    const allCompanies = await db.select().from(companies).all();
    
    if (allCompanies.length === 0) {
      console.log('❌ No companies found. Please add companies before creating reports.');
      return;
    }
    
    // Get existing report dates
    const existingReports = await db.select({
      date: reports.date
    }).from(reports).all();
    
    if (existingReports.length === 0) {
      console.log('❌ No existing reports found to use dates from.');
      return;
    }
    
    const existingDates = existingReports.map(report => report.date);
    
    const jobTypes = ['Commercial', 'Photoshoot', 'Event', 'Promo', 'Marketing', 'Video'];
    const mediaTypes = ['Instagram', 'Facebook', 'TikTok', 'TV', 'YouTube', 'Outdoor'];
    const markets = ['Europe', 'Asia', 'North America', 'Russia', 'Africa', 'Global'];
    
    // Creating 10 more reports with existing dates
    for (let i = 0; i < 10; i++) {
      // Randomly select an employee
      const employee = allEmployees[Math.floor(Math.random() * allEmployees.length)];
      
      // Randomly select 1-3 companies
      const numCompanies = Math.floor(Math.random() * 3) + 1;
      const selectedCompanyIds: number[] = [];
      
      for (let j = 0; j < numCompanies; j++) {
        const randomCompany = allCompanies[Math.floor(Math.random() * allCompanies.length)];
        if (!selectedCompanyIds.includes(randomCompany.id)) {
          selectedCompanyIds.push(randomCompany.id);
        }
      }
      
      // Use an existing date from the first 10 reports
      // If there are fewer than 10 reports, reuse dates as needed
      const dateIndex = i % existingDates.length;
      const reportDate = existingDates[dateIndex];
      
      // Random report data
      const reportData = {
        employeeId: employee.id,
        date: reportDate,
        market: markets[Math.floor(Math.random() * markets.length)],
        contractingAgency: (allCompanies[Math.floor(Math.random() * allCompanies.length)]).name,
        client: `Client ${Math.floor(Math.random() * 100)}`,
        projectBrand: `Project ${Math.floor(Math.random() * 50)}`,
        media: mediaTypes[Math.floor(Math.random() * mediaTypes.length)],
        jobType: jobTypes[Math.floor(Math.random() * jobTypes.length)],
        comments: `Additional report ${i + 1}. Work completed successfully.`,
        hours: Math.floor(Math.random() * 8) + 1, // 1-8 hours
      };
      
      // Insert the report and create company associations
      await db.transaction(async (tx) => {
        // Insert the report
        const newReport = await tx
          .insert(reports)
          .values(reportData)
          .returning()
          .get();
        
        // Create company associations
        if (selectedCompanyIds.length > 0) {
          await tx
            .insert(reportsToCompanies)
            .values(
              selectedCompanyIds.map(companyId => ({
                reportId: newReport.id,
                companyId,
              }))
            );
        }
        
        console.log(`✅ Additional Report ${i + 1} added for employee ${employee.name} on ${reportDate}`);
      });
    }
    
    console.log('\n✅ Successfully added 10 more reports to the database!');
    
  } catch (error) {
    console.error('❌ Failed to load reports:', error);
  } finally {
    sqlite.close();
  }
}

// Execute the function
loadReports(); 
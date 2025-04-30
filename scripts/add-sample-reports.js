import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import { reports, reportsToCompanies, employees, companies } from '../db/schema.ts';
import path from 'path';
import { fileURLToPath } from 'url';
import { eq } from 'drizzle-orm';

// Get the directory name
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Initialize the database
const sqlite = new Database(path.resolve(__dirname, '../data.db'));
const db = drizzle(sqlite);

async function generateRandomDate(start, end) {
  const date = new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
  return date.toISOString().split('T')[0]; // Format as YYYY-MM-DD
}

async function addSampleReports() {
  console.log('🌱 Adding 10 sample reports to database...');
  
  try {
    // Get all employees from Бухгалтерія department
    const accountingEmployees = await db.select()
      .from(employees)
      .where(eq(employees.department, 'Бухгалтерія'))
      .all();
    
    if (accountingEmployees.length === 0) {
      console.log('❌ No employees found in Бухгалтерія department. Please add employees before creating reports.');
      return;
    }
    
    // Get all companies
    const allCompanies = await db.select().from(companies).all();
    
    if (allCompanies.length === 0) {
      console.log('❌ No companies found. Please add companies before creating reports.');
      return;
    }
    
    const jobTypes = ['Податкове планування', 'Бухгалтерський облік', 'Фінансовий аналіз', 'Складання звітності', 'Аудит', 'Консультація'];
    const projects = ['Річний звіт', 'Квартальний звіт', 'Податкова декларація', 'Баланс', 'Фінансовий план', 'Бюджетування'];
    const clients = ['ТОВ "Фінансгруп"', 'ПАТ "Укрбуд"', 'ТОВ "Агроінвест"', 'ФОП Петренко', 'ПП "Надія"', 'ТОВ "Логістика Плюс"'];
    const mediaTypes = [
      'All media',
      'OOH',
      'Other',
      'Print',
      'Radio',
      'Research',
      'Trading',
      'TV',
      'Video',
      'Digital - all',
      'Digital - Paid Social',
      'Digital - Search',
      'Digital - Display',
      'Digital - Video',
      'Digital SP',
      'Digital Influencers',
      'Digital other'
    ];
    
    // Generate dates for the last 2 months
    const endDate = new Date();
    const startDate = new Date();
    startDate.setMonth(endDate.getMonth() - 2);
    
    // Creating 10 reports
    for (let i = 0; i < 10; i++) {
      // Randomly select an employee
      const employee = accountingEmployees[Math.floor(Math.random() * accountingEmployees.length)];
      
      // Randomly select 1-2 companies
      const numCompanies = Math.floor(Math.random() * 2) + 1;
      const selectedCompanyIds = [];
      
      for (let j = 0; j < numCompanies; j++) {
        const randomCompany = allCompanies[Math.floor(Math.random() * allCompanies.length)];
        if (!selectedCompanyIds.includes(randomCompany.id)) {
          selectedCompanyIds.push(randomCompany.id);
        }
      }
      
      // Generate a random date
      const reportDate = await generateRandomDate(startDate, endDate);
      
      // Random client from the list
      const randomClient = clients[Math.floor(Math.random() * clients.length)];
      
      // Random report data
      const reportData = {
        employeeId: employee.id,
        date: reportDate,
        market: 'Україна',
        contractingAgency: employee.agency || 'MediaCom',
        client: randomClient,
        projectBrand: projects[Math.floor(Math.random() * projects.length)],
        media: mediaTypes[Math.floor(Math.random() * mediaTypes.length)],
        jobType: jobTypes[Math.floor(Math.random() * jobTypes.length)],
        comments: `Звіт #${i + 1}. ${Math.random() > 0.5 ? 'Робота завершена.' : 'Виконано частково, потребує доопрацювання.'}`,
        hours: Math.floor(Math.random() * 6) + 2, // 2-8 hours
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
        
        console.log(`✅ Report ${i + 1} added for ${employee.name} on ${reportDate}: ${reportData.jobType}`);
      });
    }
    
    console.log('\n✅ Successfully added 10 sample reports to the database!');
    
  } catch (error) {
    console.error('❌ Failed to add sample reports:', error);
    console.error(error);
  } finally {
    sqlite.close();
  }
}

// Run the function
addSampleReports(); 
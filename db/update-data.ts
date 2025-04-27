import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import { clients, companies, employees, reports, reportsToCompanies } from './schema';
import { eq, sql } from 'drizzle-orm';

// Initialize the database
const sqlite = new Database('./data.db');
const db = drizzle(sqlite);

// Define company object type
type CompanyRecord = typeof companies.$inferSelect;
type ReportData = Omit<typeof reports.$inferInsert, 'id'> & {
  companies?: string[];
};

async function updateDatabase() {
  console.log('🔄 Updating database with specific data...');
  
  try {
    // Clear existing data
    console.log('🧹 Clearing existing data...');
    await db.delete(reportsToCompanies);
    await db.delete(reports);
    await db.delete(employees);
    
    // Insert the two required employees
    console.log('👥 Inserting specific employees...');
    const adminEmployee = await db.insert(employees).values({
      name: 'Andrey Admin',
      email: 'st6288743@stud.duikt.edu.ua',
      position: 'Admin',
      department: 'GroupM',
      joinDate: new Date().toISOString().split('T')[0],
      status: 'Active',
    }).returning().get();
    
    const userEmployee = await db.insert(employees).values({
      name: 'Andrey User',
      email: 'anruban2001@gmail.com',
      position: 'Specialist',
      department: 'MediaCom',
      joinDate: new Date().toISOString().split('T')[0],
      status: 'Active',
    }).returning().get();
    
    console.log('🏢 Updating company information...');
    
    // Get existing companies or create new ones
    let groupMCompany = await db.select().from(companies).where(eq(companies.name, 'GroupM')).get();
    let mediaComCompany = await db.select().from(companies).where(eq(companies.name, 'MediaCom')).get();
    
    if (!groupMCompany) {
      groupMCompany = await db.insert(companies).values({
        name: 'GroupM',
        contact: 'N/A',
        email: 'contact@groupm.com',
        phone: 'N/A',
        projects: 3,
        address: 'N/A',
        notes: 'Main holding company',
      }).returning().get();
    } else {
      await db.update(companies).set({
        contact: 'N/A',
        email: 'contact@groupm.com',
        phone: 'N/A',
        projects: 3,
        address: 'N/A',
        notes: 'Main holding company',
      }).where(eq(companies.id, groupMCompany.id));
    }
    
    if (!mediaComCompany) {
      mediaComCompany = await db.insert(companies).values({
        name: 'MediaCom',
        contact: 'N/A',
        email: 'contact@mediacom.com',
        phone: 'N/A',
        projects: 2,
        address: 'N/A',
        notes: 'Media agency under GroupM',
      }).returning().get();
    } else {
      await db.update(companies).set({
        contact: 'N/A',
        email: 'contact@mediacom.com',
        phone: 'N/A',
        projects: 2,
        address: 'N/A',
        notes: 'Media agency under GroupM',
      }).where(eq(companies.id, mediaComCompany.id));
    }
    
    // Add a few more companies
    const clientCompanies = [
      {
        name: 'Coca-Cola',
        contact: 'N/A',
        email: 'contact@cocacola.com',
        phone: 'N/A',
        projects: 2,
        address: 'N/A',
        notes: 'Beverage company client',
      },
      {
        name: 'Samsung',
        contact: 'N/A',
        email: 'contact@samsung.com',
        phone: 'N/A',
        projects: 1,
        address: 'N/A',
        notes: 'Electronics company client',
      },
      {
        name: 'Nestlé',
        contact: 'N/A',
        email: 'contact@nestle.com',
        phone: 'N/A',
        projects: 1,
        address: 'N/A',
        notes: 'Food and beverage company client',
      }
    ];
    
    // Store company objects for reference later
    const companyObjects: Record<string, CompanyRecord> = {};
    
    for (const company of clientCompanies) {
      const existingCompany = await db.select().from(companies).where(eq(companies.name, company.name)).get();
      
      if (!existingCompany) {
        const newCompany = await db.insert(companies).values(company).returning().get();
        companyObjects[company.name] = newCompany;
      } else {
        await db.update(companies).set(company).where(eq(companies.id, existingCompany.id));
        companyObjects[company.name] = existingCompany;
      }
    }
    
    // Make sure we have references to our agency companies
    if (groupMCompany) companyObjects['GroupM'] = groupMCompany;
    if (mediaComCompany) companyObjects['MediaCom'] = mediaComCompany;
    
    // Create reports for admin employee
    console.log('📊 Creating reports for Admin employee...');
    const adminReports: ReportData[] = [
      {
        employeeId: adminEmployee.id,
        date: '2024-06-15', // Fixed date
        market: 'Ukraine',
        contractingAgency: 'GroupM',
        client: 'Coca-Cola',
        projectBrand: 'Coca-Cola Classic',
        media: 'Digital',
        jobType: 'Planning',
        comments: 'Campaign planning for Q3',
        hours: 6,
        companies: ['GroupM', 'Coca-Cola']
      },
      {
        employeeId: adminEmployee.id,
        date: '2024-06-16', // Fixed date
        market: 'Ukraine',
        contractingAgency: 'GroupM',
        client: 'Samsung',
        projectBrand: 'Galaxy S24',
        media: 'Social Media',
        jobType: 'Strategy',
        comments: 'Social media strategy development',
        hours: 8,
        companies: ['GroupM', 'Samsung']
      },
      {
        employeeId: adminEmployee.id,
        date: '2024-06-17', // Fixed date
        market: 'Ukraine',
        contractingAgency: 'GroupM',
        client: 'Nestlé',
        projectBrand: 'Nescafé',
        media: 'TV',
        jobType: 'Buying',
        comments: 'TV spots purchase for holiday campaign',
        hours: 4,
        companies: ['GroupM', 'Nestlé']
      },
    ];
    
    // Create and store each report, then create the company associations
    for (const reportData of adminReports) {
      // Extract and store companies for report
      const companiesForReport = reportData.companies ? [...reportData.companies] : [];
      
      // Create a new object without the companies property
      const { companies: _, ...reportDataWithoutCompanies } = reportData;
      
      // Insert the report
      const newReport = await db.insert(reports).values(reportDataWithoutCompanies).returning().get();
      
      // Create report-to-company associations
      for (const companyName of companiesForReport) {
        if (companyObjects[companyName]) {
          await db.insert(reportsToCompanies).values({
            reportId: newReport.id,
            companyId: companyObjects[companyName].id
          });
        } else {
          console.warn(`Company "${companyName}" not found for report association`);
        }
      }
    }
    
    // Create reports for user employee
    console.log('📊 Creating reports for User employee...');
    const userReports: ReportData[] = [
      {
        employeeId: userEmployee.id,
        date: '2024-06-14', // Fixed date
        market: 'Ukraine',
        contractingAgency: 'MediaCom',
        client: 'Samsung',
        projectBrand: 'Galaxy Watch',
        media: 'Display',
        jobType: 'Implementation',
        comments: 'Banner campaign setup',
        hours: 5,
        companies: ['MediaCom', 'Samsung']
      },
      {
        employeeId: userEmployee.id,
        date: '2024-06-16', // Fixed date
        market: 'Ukraine',
        contractingAgency: 'MediaCom',
        client: 'Coca-Cola',
        projectBrand: 'Sprite',
        media: 'Social Media',
        jobType: 'Reporting',
        comments: 'Monthly performance report creation',
        hours: 7,
        companies: ['MediaCom', 'Coca-Cola']
      },
      {
        employeeId: userEmployee.id,
        date: '2024-06-17', // Fixed date
        market: 'Ukraine',
        contractingAgency: 'MediaCom',
        client: 'Coca-Cola',
        projectBrand: 'Fanta',
        media: 'Mobile',
        jobType: 'Analytics',
        comments: 'Campaign analysis and optimization',
        hours: 6,
        companies: ['MediaCom', 'Coca-Cola']
      },
    ];
    
    // Create and store each report, then create the company associations for user reports
    for (const reportData of userReports) {
      // Extract and store companies for report
      const companiesForReport = reportData.companies ? [...reportData.companies] : [];
      
      // Create a new object without the companies property
      const { companies: _, ...reportDataWithoutCompanies } = reportData;
      
      // Insert the report
      const newReport = await db.insert(reports).values(reportDataWithoutCompanies).returning().get();
      
      // Create report-to-company associations
      for (const companyName of companiesForReport) {
        if (companyObjects[companyName]) {
          await db.insert(reportsToCompanies).values({
            reportId: newReport.id,
            companyId: companyObjects[companyName].id
          });
        } else {
          console.warn(`Company "${companyName}" not found for report association`);
        }
      }
    }
    
    // Count how many report-to-company associations we created
    const associationsCount = await db.select({ count: sql`count(*)` }).from(reportsToCompanies).get();
    
    console.log('\n✅ Database update completed successfully!');
    console.log(`📊 Summary:`);
    console.log(`  - Added 2 employees`);
    console.log(`  - Created 6 reports (3 per employee)`);
    console.log(`  - Created ${associationsCount?.count || 0} report-to-company associations`);
    console.log(`  - Updated company information`);
    
  } catch (error) {
    console.error('❌ Database update failed:', error);
    console.error(error);
  } finally {
    sqlite.close();
  }
}

updateDatabase();
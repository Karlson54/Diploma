import { drizzle } from 'drizzle-orm/better-sqlite3';
import Database from 'better-sqlite3';
import { companies } from './schema';
import { eq } from 'drizzle-orm';

// Initialize the database
const sqlite = new Database('./data.db');
const db = drizzle(sqlite);

// Company data with updated information
const companyUpdates = [
  {
    name: 'GroupM',
    contact: 'John Smith',
    email: 'contact@groupm.com',
    phone: '+380 44 123 4567',
    address: 'Khreshchatyk St, 22, Kyiv, 01001, Ukraine',
    notes: 'Main holding company for media agencies. Works with multiple international brands.'
  },
  {
    name: 'MediaCom',
    contact: 'Anna Johnson',
    email: 'contact@mediacom.com',
    phone: '+380 44 234 5678',
    address: 'Velyka Vasylkivska St, 100, Kyiv, 03150, Ukraine',
    notes: 'Media agency under GroupM specializing in digital and traditional media buying.'
  },
  {
    name: 'Coca-Cola',
    contact: 'Michael Brown',
    email: 'contact@cocacola.com',
    phone: '+380 44 345 6789',
    address: 'Brovarskyi Ave, 5, Kyiv, 02099, Ukraine',
    notes: 'Global beverage company, primary focus on soft drinks and promotional campaigns.'
  },
  {
    name: 'Samsung',
    contact: 'Sarah Wilson',
    email: 'contact@samsung.com',
    phone: '+380 44 456 7890',
    address: 'Lva Tolstoho St, 57, Kyiv, 01032, Ukraine',
    notes: 'Electronics company focusing on mobile devices, TVs, and home appliances marketing.'
  },
  {
    name: 'Nestlé',
    contact: 'David Taylor',
    email: 'contact@nestle.com',
    phone: '+380 44 567 8901',
    address: 'Verkhnii Val St, 72, Kyiv, 04071, Ukraine', 
    notes: 'Food and beverage company with multiple brands including coffee, confectionery, and dairy.'
  }
];

async function updateCompanyData() {
  console.log('🔄 Updating company data to replace N/A values...');
  
  try {
    for (const companyData of companyUpdates) {
      // Find the company by name
      const existingCompany = await db.select().from(companies).where(eq(companies.name, companyData.name)).get();
      
      if (existingCompany) {
        console.log(`🏢 Updating company: ${companyData.name}`);
        // Update the company data
        await db.update(companies)
          .set({
            contact: companyData.contact,
            email: companyData.email,
            phone: companyData.phone,
            address: companyData.address,
            notes: companyData.notes
          })
          .where(eq(companies.id, existingCompany.id));
      } else {
        console.log(`❌ Company not found: ${companyData.name}`);
      }
    }
    
    // Get all companies to check for any remaining N/A values
    const allCompanies = await db.select().from(companies).all();
    
    // Check for any company that might still have N/A values and update them
    for (const company of allCompanies) {
      const needsUpdate = 
        company.contact === 'N/A' || 
        company.phone === 'N/A' || 
        company.address === 'N/A' || 
        company.notes === 'N/A';
      
      if (needsUpdate) {
        console.log(`🔍 Found company with N/A values: ${company.name}`);
        
        await db.update(companies)
          .set({
            contact: company.contact === 'N/A' ? 'Contact Person' : company.contact,
            phone: company.phone === 'N/A' ? '+380 44 999 9999' : company.phone,
            address: company.address === 'N/A' ? 'Kyiv, Ukraine' : company.address,
            notes: company.notes === 'N/A' ? 'Client company' : company.notes
          })
          .where(eq(companies.id, company.id));
        
        console.log(`✅ Updated company: ${company.name}`);
      }
    }
    
    console.log('✅ All company data has been updated successfully!');
    
    // Print the updated companies
    const updatedCompanies = await db.select().from(companies).all();
    console.log('📋 Updated Companies:');
    console.table(updatedCompanies);
    
  } catch (error) {
    console.error('❌ Error updating company data:', error);
  } finally {
    // Close the database connection
    sqlite.close();
  }
}

// Run the update function
updateCompanyData(); 
import { db } from './index';
import { eq } from 'drizzle-orm';
import { employees } from './schema';

async function updateEmployeeAgencies() {
  console.log('🔄 Starting update process...');
  
  try {
    // Get all employees
    const allEmployees = await db.select().from(employees).all();
    console.log(`📊 Found ${allEmployees.length} employees to process.`);
    
    let updatedCount = 0;
    
    for (const employee of allEmployees) {
      // Update each employee to ensure agency field is set
      
      // If agency is not set, use department as the agency
      if (!employee.agency) {
        await db.update(employees)
          .set({ agency: employee.department })
          .where(eq(employees.id, employee.id))
          .run();
          
        updatedCount++;
        
        console.log(`✅ Updated employee: ${employee.name} - Set agency to: ${employee.department}`);
      }
    }
    
    console.log(`\n🎉 Update complete! Updated ${updatedCount} employees.`);
  } catch (error) {
    console.error('❌ Update failed:', error);
  }
}

updateEmployeeAgencies(); 
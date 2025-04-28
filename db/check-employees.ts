import { db } from './index';
import { employees } from './schema';

async function checkEmployeeAgencies() {
  console.log('🔎 Checking employee data...');
  
  try {
    // Get all employees
    const allEmployees = await db.select().from(employees).all();
    console.log(`📊 Found ${allEmployees.length} employees in the database.`);
    
    // Basic statistics
    const activeEmployees = allEmployees.filter(e => e.status === 'active').length;
    console.log(`📊 Active employees: ${activeEmployees}`);
    
    // Log a sample of employees with their agency field
    console.log('\n📋 Sample employees with agency data:');
    
    // Show up to 5 employees as a sample
    for (let i = 0; i < Math.min(5, allEmployees.length); i++) {
      const employee = allEmployees[i];
      console.log(`- ${employee.name}: Agency = "${employee.agency || 'NOT SET'}"`);
    }
    
    // Count employees with agency field set
    const employeesWithAgency = allEmployees.filter(e => e.agency).length;
    console.log(`\n📊 Summary: ${employeesWithAgency} out of ${allEmployees.length} employees have agency field set (${Math.round(employeesWithAgency / allEmployees.length * 100)}%)`);
    
    console.log('\n✅ Check completed successfully!');
  } catch (error) {
    console.error('❌ Check failed:', error);
  }
}

checkEmployeeAgencies(); 
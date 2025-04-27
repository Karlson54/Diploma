import { NextResponse } from 'next/server';
import { employeeQueries, companyQueries, reportQueries } from '@/db/queries';

// Get dashboard stats
export async function GET(request: Request) {
  try {
    const employees = await employeeQueries.getAll();
    const companies = await companyQueries.getAll();
    
    return NextResponse.json({
      employeeCount: employees.length,
      companyCount: companies.length
    });
  } catch (error) {
    console.error("API Error:", error);
    return NextResponse.json({ error: "Failed to fetch dashboard data" }, { status: 500 });
  }
}

// Employee data endpoint
export async function POST(request: Request) {
  try {
    const body = await request.json();
    const { action } = body;
    
    if (action === 'getEmployees') {
      const employees = await employeeQueries.getAll();
      return NextResponse.json({ employees });
    } 
    else if (action === 'addEmployee') {
      const { employee } = body;
      // TODO: Implement actual DB insertion
      const newEmployeeId = Math.floor(Math.random() * 10000) + 100; // Mock ID generation
      return NextResponse.json({ id: newEmployeeId, success: true });
    }
    else if (action === 'updateEmployee') {
      const { employee } = body;
      // TODO: Implement actual DB update
      return NextResponse.json({ success: true });
    }
    else if (action === 'deleteEmployee') {
      const { id } = body;
      // TODO: Implement actual DB deletion
      return NextResponse.json({ success: true });
    }
    else if (action === 'getCompanies') {
      const companies = await companyQueries.getAll();
      const allReports = await reportQueries.getAllWithEmployee();
      
      // Transform companies with calculated statistics
      const companiesWithStats = companies.map(company => {
        // Find reports related to this company
        const companyReports = allReports.filter(r => 
          r.report.client?.includes(company.name) || 
          r.report.contractingAgency?.includes(company.name)
        );
        
        // Calculate total hours (as a simple revenue metric)
        const totalHours = companyReports.reduce((sum, r) => sum + r.report.hours, 0);
        // For this example, assume each hour is worth 1000₴
        const revenue = totalHours * 1000;
        
        return {
          ...company,
          activeProjects: company.projects || 0,
          totalHours,
          revenue
        };
      });
      
      return NextResponse.json({ companies: companiesWithStats });
    }
    else if (action === 'addCompany') {
      const { company } = body;
      // TODO: Implement actual DB insertion
      const newCompanyId = Math.floor(Math.random() * 10000) + 100; // Mock ID generation
      return NextResponse.json({ id: newCompanyId, success: true });
    }
    else if (action === 'updateCompany') {
      const { company } = body;
      // TODO: Implement actual DB update
      return NextResponse.json({ success: true });
    }
    else if (action === 'deleteCompany') {
      const { id } = body;
      // TODO: Implement actual DB deletion
      return NextResponse.json({ success: true });
    }
    else if (action === 'getReports') {
      const allReports = await reportQueries.getAllWithEmployee();
      
      return NextResponse.json({ reports: allReports });
    }
    else {
      return NextResponse.json({ error: "Invalid action" }, { status: 400 });
    }
  } catch (error) {
    console.error("API Error:", error);
    return NextResponse.json({ error: "Failed to process request" }, { status: 500 });
  }
} 
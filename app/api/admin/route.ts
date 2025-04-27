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
      
      try {
        // Insert the new company into the database
        const newCompany = await companyQueries.add({
          name: company.name,
          contact: company.contact,
          email: company.email,
          phone: company.phone,
          projects: 0, // Start with 0 projects
          address: company.address || null,
          notes: company.notes || null
        });
        
        return NextResponse.json({ id: newCompany.id, success: true });
      } catch (error) {
        console.error("Error adding company:", error);
        return NextResponse.json({ error: "Failed to add company" }, { status: 500 });
      }
    }
    else if (action === 'updateCompany') {
      const { company } = body;
      
      try {
        // Update the company in the database
        const updatedCompany = await companyQueries.update(company.id, {
          name: company.name,
          contact: company.contact,
          email: company.email,
          phone: company.phone,
          address: company.address || null,
          notes: company.notes || null
        });
        
        return NextResponse.json({ success: true, company: updatedCompany });
      } catch (error) {
        console.error("Error updating company:", error);
        return NextResponse.json({ error: "Failed to update company" }, { status: 500 });
      }
    }
    else if (action === 'deleteCompany') {
      const { id } = body;
      
      try {
        // Delete the company from the database
        const deletedCompany = await companyQueries.delete(id);
        
        if (!deletedCompany) {
          return NextResponse.json({ error: "Company not found" }, { status: 404 });
        }
        
        return NextResponse.json({ 
          success: true, 
          message: `Company ${deletedCompany.name} successfully deleted` 
        });
      } catch (error) {
        console.error("Error deleting company:", error);
        return NextResponse.json({ error: "Failed to delete company" }, { status: 500 });
      }
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
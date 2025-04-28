import { NextResponse } from 'next/server';
import { employeeQueries, companyQueries, reportQueries } from '@/db/queries';
import { auth } from '@clerk/nextjs/server';
import { clerkClient } from '@clerk/nextjs/server'

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
  const client = await clerkClient()
  try {
    const body = await request.json();
    const { action } = body;
    
    if (action === 'getEmployees') {
      const employees = await employeeQueries.getAll();
      return NextResponse.json({ employees });
    } 
    else if (action === 'addEmployee') {
      const { employee } = body;
      
      try {
        // First verify that the requester is admin
        const { userId } = await auth();
        
        if (!userId) {
          return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
        }
        
        // Get the admin user to check if they have admin role
        const adminUser = await client.users.getUser(userId);
        const isAdmin = adminUser.publicMetadata.role === 'admin';
        
        if (!isAdmin) {
          return NextResponse.json({ error: 'Forbidden: Admin access required' }, { status: 403 });
        }
        
        // Required fields check
        if (!employee.email || !employee.password || !employee.name) {
          return NextResponse.json({ error: 'Email, password and name are required' }, { status: 400 });
        }
        
        // First create a user in Clerk
        const newUser = await client.users.createUser({
          emailAddress: [employee.email],
          password: employee.password,
        });
        
        
        // Then add the employee to the database
        const today = new Date().toISOString();
        
        const newEmployee = await employeeQueries.add({
          name: employee.name,
          email: employee.email,
          position: employee.position,
          department: employee.department,
          joinDate: today,
          status: employee.status,
          clerkId: newUser.id,
        });
        
        return NextResponse.json({ 
          id: newEmployee.id, 
          userId: newUser.id,
          success: true 
        });
      } catch (error: any) {
        console.error("Error adding employee:", error);
        console.log(error.errors);
        return NextResponse.json({ 
          error: error.message || "Failed to add employee" 
        }, { 
          status: error.status || 500 
        });
        
      }
    }
    else if (action === 'updateEmployee') {
      const { employee } = body;
      // TODO: Implement actual DB update
      return NextResponse.json({ success: true });
    }
    else if (action === 'deleteEmployee') {
      const { id } = body;
      
      try {
        // Get the employee to find their Clerk ID
        const employee = await employeeQueries.getById(id);
        
        if (!employee) {
          return NextResponse.json({ error: "Employee not found" }, { status: 404 });
        }
        
        // Delete the employee from the database
        const deletedEmployee = await employeeQueries.delete(id);
        
        // If employee has a clerkId, delete from Clerk as well
        if (employee.clerkId) {
          try {
            await client.users.deleteUser(employee.clerkId);
          } catch (clerkError) {
            console.error("Error deleting user from Clerk:", clerkError);
            // Continue anyway as we've already deleted from our DB
          }
        }
        
        return NextResponse.json({ 
          success: true, 
          message: `Employee ${employee.name} successfully deleted` 
        });
      } catch (error) {
        console.error("Error deleting employee:", error);
        return NextResponse.json({ error: "Failed to delete employee" }, { status: 500 });
      }
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
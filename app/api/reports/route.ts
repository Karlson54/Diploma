import { NextResponse } from 'next/server';
import { employeeQueries, reportQueries, companyQueries } from '@/db/queries';
import { auth } from '@clerk/nextjs/server';

// Get user's reports
export async function GET(request: Request) {
  try {
    // Get all reports with employee data and related company information
    const reports = await reportQueries.getAllWithEmployee();
    
    // For each report, ensure we have all the data needed for export
    const enrichedReports = await Promise.all(
      reports.map(async (report) => {
        // Get additional details for each report if needed
        const detailedReport = await reportQueries.getByIdWithDetails(report.report.id);
        
        // Make sure we have complete employee data
        let employeeData = report.employee;
        if (employeeData && !employeeData.name) {
          const employee = await employeeQueries.getById(employeeData.id);
          if (employee) {
            employeeData = {
              id: employeeData.id,
              name: employee.name || '-',
              agency: employeeData.agency || '-'
            };
          }
        }
        
        // Check if we have agency data
        if (employeeData && (!employeeData.agency || employeeData.agency === 'null')) {
          const employee = await employeeQueries.getById(employeeData.id);
          if (employee && employee.agency) {
            employeeData.agency = employee.agency;
          } else {
            employeeData.agency = '-';
          }
        }
        
        // Ensure we have market data
        let market = report.report.market || '-';
        if (!market || market === 'null') {
          // Try to get market from detailed report
          if (detailedReport?.report.market) {
            market = detailedReport.report.market;
          }
        }
        
        // Return the report with enhanced data
        const enhancedReport = {
          ...report,
          employee: employeeData,
          report: {
            ...report.report,
            // Убедимся, что у нас есть все необходимые данные из БД
            projectBrand: report.report.projectBrand || detailedReport?.report.projectBrand || '-',
            market: market,
            media: report.report.media || '-',
            comments: report.report.comments || '-',
            client: report.report.client || '-',
            contractingAgency: report.report.contractingAgency || '-',
            jobType: report.report.jobType || '-',
          },
          // Add any companies associated with the report
          companies: detailedReport?.companies || []
        };
        
        return enhancedReport;
      })
    );
    
    return NextResponse.json(enrichedReports);
  } catch (error) {
    console.error('Error fetching reports:', error);
    return NextResponse.json({ error: 'Failed to fetch reports' }, { status: 500 });
  }
}

// Create a new report
export async function POST(request: Request) {
  try {
    // Get the current user's ID
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }
    
    // Parse request body
    const body = await request.json();
    const { date, market, contractingAgency, client, projectBrand, media, jobType, comments, hours } = body;
    
    // Find the employee record for the current user
    const employees = await employeeQueries.getAll();
    const currentEmployee = employees.find(emp => emp.clerkId === userId);
    
    if (!currentEmployee) {
      return NextResponse.json({ error: 'Employee record not found' }, { status: 404 });
    }
    
    // Prepare report data
    const reportData = {
      employeeId: currentEmployee.id,
      date,
      market,
      contractingAgency,
      client,
      projectBrand,
      media,
      jobType,
      comments,
      hours: Number(hours) || 0,
    };
    
    // Get company IDs if provided
    let companyIds: number[] = [];
    if (body.companies && Array.isArray(body.companies) && body.companies.length > 0) {
      // If company names are provided, find their IDs
      companyIds = await getCompanyIds(body.companies);
    } else {
      // Check if client name or contracting agency names match companies
      const companies = await companyQueries.getAll();
      const clientCompany = companies.find(c => c.name === client);
      const agencyCompany = companies.find(c => c.name === contractingAgency);
      
      if (clientCompany) companyIds.push(clientCompany.id);
      if (agencyCompany && agencyCompany.id !== (clientCompany?.id || 0)) {
        companyIds.push(agencyCompany.id);
      }
    }
    
    // Add the new report
    const newReport = await reportQueries.add(reportData, companyIds);
    
    return NextResponse.json({ success: true, report: newReport });
  } catch (error) {
    console.error("API Error:", error);
    return NextResponse.json({ error: "Failed to create report" }, { status: 500 });
  }
}

// Update a report
export async function PUT(request: Request) {
  try {
    // Get the current user's ID
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }
    
    // Parse request body
    const body = await request.json();
    const { id, date, market, contractingAgency, client, projectBrand, media, jobType, comments, hours } = body;
    
    if (!id) {
      return NextResponse.json({ error: 'Report ID is required' }, { status: 400 });
    }
    
    // Find the employee record for the current user
    const employees = await employeeQueries.getAll();
    const currentEmployee = employees.find(emp => emp.clerkId === userId);
    
    if (!currentEmployee) {
      return NextResponse.json({ error: 'Employee record not found' }, { status: 404 });
    }
    
    // Verify the report belongs to this user
    const report = await reportQueries.getByIdWithDetails(id);
    if (!report || report.report.employeeId !== currentEmployee.id) {
      return NextResponse.json({ error: 'Report not found or access denied' }, { status: 403 });
    }
    
    // Prepare report data for update
    const reportData = {
      date,
      market,
      contractingAgency,
      client,
      projectBrand,
      media,
      jobType,
      comments,
      hours: Number(hours) || 0,
    };
    
    // Get company IDs if provided
    let companyIds: number[] | undefined = undefined;
    if (body.companies !== undefined) {
      if (Array.isArray(body.companies)) {
        // If company names are provided, find their IDs
        companyIds = await getCompanyIds(body.companies);
      } else {
        companyIds = [];
        // Check if client name or contracting agency names match companies
        const companies = await companyQueries.getAll();
        const clientCompany = companies.find(c => c.name === client);
        const agencyCompany = companies.find(c => c.name === contractingAgency);
        
        if (clientCompany) companyIds.push(clientCompany.id);
        if (agencyCompany && agencyCompany.id !== (clientCompany?.id || 0)) {
          companyIds.push(agencyCompany.id);
        }
      }
    }
    
    // Update the report
    const updatedReport = await reportQueries.update(id, reportData, companyIds);
    
    return NextResponse.json({ success: true, report: updatedReport });
  } catch (error) {
    console.error("API Error:", error);
    return NextResponse.json({ error: "Failed to update report" }, { status: 500 });
  }
}

// Delete a report
export async function DELETE(request: Request) {
  try {
    // Get the current user's ID
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }
    
    // Get the report ID from the URL
    const { searchParams } = new URL(request.url);
    const id = searchParams.get('id');
    
    if (!id) {
      return NextResponse.json({ error: 'Report ID is required' }, { status: 400 });
    }
    
    // Find the employee record for the current user
    const employees = await employeeQueries.getAll();
    const currentEmployee = employees.find(emp => emp.clerkId === userId);
    
    if (!currentEmployee) {
      return NextResponse.json({ error: 'Employee record not found' }, { status: 404 });
    }
    
    // Verify the report belongs to this user
    const report = await reportQueries.getByIdWithDetails(parseInt(id, 10));
    if (!report || report.report.employeeId !== currentEmployee.id) {
      return NextResponse.json({ error: 'Report not found or access denied' }, { status: 403 });
    }
    
    // Delete the report
    await reportQueries.delete(parseInt(id, 10));
    
    return NextResponse.json({ success: true });
  } catch (error) {
    console.error("API Error:", error);
    return NextResponse.json({ error: "Failed to delete report" }, { status: 500 });
  }
}

// Helper function to get company IDs from company names
async function getCompanyIds(companyNames: string[]): Promise<number[]> {
  const companies = await companyQueries.getAll();
  const companyIds: number[] = [];
  
  for (const name of companyNames) {
    const company = companies.find(c => c.name === name);
    if (company) {
      companyIds.push(company.id);
    }
  }
  
  return companyIds;
} 
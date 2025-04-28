import { and, desc, eq, sql } from 'drizzle-orm';
import { db } from './index';
import { clients, companies, employees, reports, reportsToCompanies } from './schema';

/**
 * Employee-related queries
 */
export const employeeQueries = {
  // Get all employees
  getAll: async () => {
    return db.select().from(employees).all();
  },
  
  // Get employee by ID
  getById: async (id: number) => {
    return db.select().from(employees).where(eq(employees.id, id)).get();
  },
  
  // Add a new employee
  add: async (data: Omit<typeof employees.$inferInsert, 'id'>) => {
    return db.insert(employees).values(data).returning().get();
  },
  
  // Update an employee
  update: async (id: number, data: Partial<typeof employees.$inferInsert>) => {
    return db.update(employees).set(data).where(eq(employees.id, id)).returning().get();
  },
  
  // Delete an employee
  delete: async (id: number) => {
    return db.delete(employees).where(eq(employees.id, id)).returning().get();
  }
};

/**
 * Company-related queries
 */
export const companyQueries = {
  // Get all companies
  getAll: async () => {
    return db.select().from(companies).all();
  },
  
  // Get company by ID
  getById: async (id: number) => {
    return db.select().from(companies).where(eq(companies.id, id)).get();
  },
  
  // Add a new company
  add: async (data: Omit<typeof companies.$inferInsert, 'id'>) => {
    return db.insert(companies).values(data).returning().get();
  },
  
  // Update a company
  update: async (id: number, data: Partial<typeof companies.$inferInsert>) => {
    try {
      // Make sure we have valid data to update
      if (Object.keys(data).length === 0) {
        throw new Error('No data provided for update');
      }
      
      // Get the existing company to verify it exists
      const existingCompany = await db.select()
        .from(companies)
        .where(eq(companies.id, id))
        .get();
        
      if (!existingCompany) {
        throw new Error(`Company with ID ${id} not found`);
      }
      
      // Perform the update
      const updated = await db.update(companies)
        .set(data)
        .where(eq(companies.id, id))
        .returning()
        .get();
        
      return updated;
    } catch (error) {
      console.error("Error updating company:", error);
      throw error;
    }
  },
  
  // Delete a company
  delete: async (id: number) => {
    return db.delete(companies).where(eq(companies.id, id)).returning().get();
  }
};

/**
 * Client-related queries
 */
export const clientQueries = {
  // Get all clients
  getAll: async () => {
    return db.select().from(clients).all();
  },
  
  // Get client by ID
  getById: async (id: number) => {
    return db.select().from(clients).where(eq(clients.id, id)).get();
  },
  
  // Add a new client
  add: async (data: Omit<typeof clients.$inferInsert, 'id'>) => {
    return db.insert(clients).values(data).returning().get();
  },
  
  // Update a client
  update: async (id: number, data: Partial<typeof clients.$inferInsert>) => {
    return db.update(clients).set(data).where(eq(clients.id, id)).returning().get();
  },
  
  // Delete a client
  delete: async (id: number) => {
    return db.delete(clients).where(eq(clients.id, id)).returning().get();
  }
};

/**
 * Report-related queries
 */
export const reportQueries = {
  // Get all reports
  getAll: async () => {
    return db.select().from(reports).all();
  },
  
  // Get reports with employee information
  getAllWithEmployee: async () => {
    return db
      .select({
        report: reports,
        employee: {
          id: employees.id,
          name: employees.name,
        },
      })
      .from(reports)
      .leftJoin(employees, eq(reports.employeeId, employees.id))
      .orderBy(desc(reports.date))
      .all();
  },
  
  // Get reports for a specific employee
  getByEmployeeId: async (employeeId: number) => {
    return db
      .select()
      .from(reports)
      .where(eq(reports.employeeId, employeeId))
      .orderBy(desc(reports.date))
      .all();
  },
  
  // Get report by ID with related data
  getByIdWithDetails: async (id: number) => {
    const report = await db
      .select({
        report: reports,
        employee: {
          id: employees.id,
          name: employees.name,
        },
      })
      .from(reports)
      .leftJoin(employees, eq(reports.employeeId, employees.id))
      .where(eq(reports.id, id))
      .get();
      
    if (!report) return null;
    
    // Get companies associated with this report
    const reportCompanies = await db
      .select({
        company: companies,
      })
      .from(reportsToCompanies)
      .leftJoin(companies, eq(reportsToCompanies.companyId, companies.id))
      .where(eq(reportsToCompanies.reportId, id))
      .all();
      
    return {
      ...report,
      companies: reportCompanies.map(rc => rc.company),
    };
  },
  
  // Add a new report with company associations
  add: async (
    reportData: Omit<typeof reports.$inferInsert, 'id'>,
    companyIds: number[]
  ) => {
    // Begin a transaction
    return db.transaction(async (tx) => {
      // Insert the report
      const newReport = await tx
        .insert(reports)
        .values(reportData)
        .returning()
        .get();
      
      // Create company associations
      if (companyIds.length > 0) {
        await tx
          .insert(reportsToCompanies)
          .values(
            companyIds.map(companyId => ({
              reportId: newReport.id,
              companyId,
            }))
          );
      }
      
      return newReport;
    });
  },
  
  // Update a report and its company associations
  update: async (
    id: number,
    reportData: Partial<typeof reports.$inferInsert>,
    companyIds?: number[]
  ) => {
    return db.transaction(async (tx) => {
      // Update the report
      const updatedReport = await tx
        .update(reports)
        .set(reportData)
        .where(eq(reports.id, id))
        .returning()
        .get();
      
      // Update company associations if provided
      if (companyIds) {
        // Delete existing associations
        await tx
          .delete(reportsToCompanies)
          .where(eq(reportsToCompanies.reportId, id));
        
        // Create new associations
        if (companyIds.length > 0) {
          await tx
            .insert(reportsToCompanies)
            .values(
              companyIds.map(companyId => ({
                reportId: id,
                companyId,
              }))
            );
        }
      }
      
      return updatedReport;
    });
  },
  
  // Delete a report and its company associations
  delete: async (id: number) => {
    return db.transaction(async (tx) => {
      // Delete report-to-company associations first
      await tx
        .delete(reportsToCompanies)
        .where(eq(reportsToCompanies.reportId, id));
      
      // Delete the report
      const deletedReport = await tx
        .delete(reports)
        .where(eq(reports.id, id))
        .returning()
        .get();
      
      return deletedReport;
    });
  }
}; 
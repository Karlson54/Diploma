import { relations } from 'drizzle-orm';
import { integer, sqliteTable, text } from 'drizzle-orm/sqlite-core';

// Employees table
export const employees = sqliteTable('employees', {
  id: integer('id').primaryKey({ autoIncrement: true }),
  name: text('name').notNull(),
  email: text('email').notNull(),
  position: text('position').notNull(),
  department: text('department').notNull(),
  joinDate: text('join_date').notNull(),
  status: text('status').notNull(),
  clerkId: text('clerk_id'),
  agency: text('agency'),
});

// Companies table
export const companies = sqliteTable('companies', {
  id: integer('id').primaryKey({ autoIncrement: true }),
  name: text('name').notNull(),
  contact: text('contact').notNull(),
  email: text('email').notNull(),
  phone: text('phone').notNull(),
  projects: integer('projects').default(0),
  address: text('address'),
  notes: text('notes'),
});

// Clients table
export const clients = sqliteTable('clients', {
  id: integer('id').primaryKey({ autoIncrement: true }),
  name: text('name').notNull(),
});

// Reports table
export const reports = sqliteTable('reports', {
  id: integer('id').primaryKey({ autoIncrement: true }),
  employeeId: integer('employee_id').references(() => employees.id, { onDelete: 'cascade' }),
  date: text('date').notNull(),
  market: text('market'),
  contractingAgency: text('contracting_agency'),
  client: text('client'),
  projectBrand: text('project_brand'),
  media: text('media'),
  jobType: text('job_type'),
  comments: text('comments'),
  hours: integer('hours').notNull(),
});

// Reports to Companies (many-to-many relationship)
export const reportsToCompanies = sqliteTable('reports_to_companies', {
  id: integer('id').primaryKey({ autoIncrement: true }),
  reportId: integer('report_id').notNull().references(() => reports.id, { onDelete: 'cascade' }),
  companyId: integer('company_id').notNull().references(() => companies.id, { onDelete: 'cascade' }),
});

// Define relations
export const employeesRelations = relations(employees, ({ many }) => ({
  reports: many(reports),
}));

export const companiesRelations = relations(companies, ({ many }) => ({
  reportsToCompanies: many(reportsToCompanies),
}));

export const reportsRelations = relations(reports, ({ one, many }) => ({
  employee: one(employees, {
    fields: [reports.employeeId],
    references: [employees.id],
  }),
  reportsToCompanies: many(reportsToCompanies),
}));

export const reportsToCompaniesRelations = relations(reportsToCompanies, ({ one }) => ({
  report: one(reports, {
    fields: [reportsToCompanies.reportId],
    references: [reports.id],
  }),
  company: one(companies, {
    fields: [reportsToCompanies.companyId],
    references: [companies.id],
  }),
})); 
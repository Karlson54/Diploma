export interface Company {
  id: string;
  name: string;
  contact: string;
  email: string;
  phone: string;
  projects: number;
  address: string;
  notes: string;
}

export interface Report {
  id: number;
  employeeId: number;
  date: string;
  market: string;
  contractingAgency: string;
  client: number;
  projectBrand: string;
  media: string;
  jobType: string;
  comments: string;
  hours: number;
}

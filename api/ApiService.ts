import { Company, Report } from "@/models/api";

class ApiService {
    // TODO: Get All Companies
    async getCompanies(): Promise<Company[]> {
        return []
    }

    // TODO: Get Reports with Employees
    async getReportsWithEmployees(): Promise<Report[]> {
        return []
    }
}

export default new ApiService()
namespace TimeTracker.Core.DTOs.Reports;

public class ExportColumnsDto
{
    public bool Agency { get; set; } = true;
    public bool Department { get; set; } = true;
    public bool FullName { get; set; } = true;
    public bool Date { get; set; } = true;
    public bool Month { get; set; } = true;
    public bool Year { get; set; } = true;
    public bool Market { get; set; } = true;
    public bool ContractingAgency { get; set; } = true;
    public bool Client { get; set; } = true;
    public bool ProjectBrand { get; set; } = true;
    public bool Media { get; set; } = true;
    public bool JobType { get; set; } = true;
    public bool Hours { get; set; } = true;
    public bool Comments { get; set; } = true;

    public static ExportColumnsDto FromString(string? columns)
    {
        if (string.IsNullOrWhiteSpace(columns))
            return new ExportColumnsDto();

        var dto = new ExportColumnsDto
        {
            Agency = false, Department = false, FullName = false, Date = false, Month = false,
            Year = false, Market = false, ContractingAgency = false, Client = false,
            ProjectBrand = false, Media = false, JobType = false, Hours = false, Comments = false
        };

        foreach (var col in columns.Split(',',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (col.ToLower())
            {
                case "agency": dto.Agency = true; break;
                case "department": dto.Department = true; break;
                case "fullname": dto.FullName = true; break;
                case "date": dto.Date = true; break;
                case "month": dto.Month = true; break;
                case "year": dto.Year = true; break;
                case "market": dto.Market = true; break;
                case "contractingagency": dto.ContractingAgency = true; break;
                case "client": dto.Client = true; break;
                case "projectbrand": dto.ProjectBrand = true; break;
                case "media": dto.Media = true; break;
                case "jobtype": dto.JobType = true; break;
                case "hours": dto.Hours = true; break;
                case "comments": dto.Comments = true; break;
            }
        }

        return dto;
    }
}
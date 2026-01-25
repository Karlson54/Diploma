namespace TimeTracker.Core.Common;

public class ExportConstants
{
    public static class Locales
    {
        public const string Ukrainian = "uk";
        public const string English = "en";
    }
    
    // Заголовки для українською
    public static class HeadersUk
    {
        // User fields
        public const string UserId = "ID Користувача";
        public const string UserName = "Ім'я користувача";
        public const string UserEmail = "Email";
        public const string AgencyName = "Агентство";
        
        // Date fields
        public const string Date = "Дата";
        public const string FromDate = "Від дати";
        public const string ToDate = "До дати";
        public const string Period = "Період";
        
        // Time fields
        public const string Hours = "Години";
        public const string TotalHours = "Всього годин";
        public const string AverageHours = "Середньо годин";
        public const string WorkingDays = "Робочих днів";
        
        // Entities
        public const string Client = "Клієнт";
        public const string Project = "Проєкт";
        public const string JobType = "Тип роботи";
        public const string Media = "Медіа";
        
        // Statistics
        public const string EntriesCount = "Кількість записів";
        public const string Percentage = "Відсоток (%)";
        public const string Total = "Разом";
        
        // Report titles
        public const string UserLoadReport = "Звіт по навантаженню користувача";
        public const string TeamLoadReport = "Звіт по навантаженню команди";
        public const string ClientReport = "Звіт по клієнту";
        public const string TimeSummaryReport = "Зведений звіт по часу";
    }
    
    // Заголовки англійською
    public static class HeadersEn
    {
        // User fields
        public const string UserId = "User ID";
        public const string UserName = "User Name";
        public const string UserEmail = "Email";
        public const string AgencyName = "Agency";
        
        // Date fields
        public const string Date = "Date";
        public const string FromDate = "From Date";
        public const string ToDate = "To Date";
        public const string Period = "Period";
        
        // Time fields
        public const string Hours = "Hours";
        public const string TotalHours = "Total Hours";
        public const string AverageHours = "Average Hours";
        public const string WorkingDays = "Working Days";
        
        // Entities
        public const string Client = "Client";
        public const string Project = "Project";
        public const string JobType = "Job Type";
        public const string Media = "Media";
        
        // Statistics
        public const string EntriesCount = "Entries Count";
        public const string Percentage = "Percentage (%)";
        public const string Total = "Total";
        
        // Report titles
        public const string UserLoadReport = "User Load Report";
        public const string TeamLoadReport = "Team Load Report";
        public const string ClientReport = "Client Report";
        public const string TimeSummaryReport = "Time Summary Report";
    }
    
    // Excel styles constants
    public static class ExcelStyles
    {
        public const string HeaderBackgroundColor = "#4472C4"; // Синій
        public const string HeaderFontColor = "#FFFFFF"; // Білий
        public const string AlternateRowColor = "#F2F2F2"; // Сірий
        public const int HeaderFontSize = 12;
        public const int DataFontSize = 10;
        public const int TitleFontSize = 14;
    }
}
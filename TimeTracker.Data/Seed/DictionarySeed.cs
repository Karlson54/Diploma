using Microsoft.Extensions.Logging;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Seed;

public class DictionarySeed : ISeeder
{
    private readonly TimeTrackerDbContext _context;
    private readonly ILogger<DictionarySeed> _logger;

    public DictionarySeed(TimeTrackerDbContext context, ILogger<DictionarySeed> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedMarketsAsync();
        await SeedAgenciesAsync();
        await SeedContractingAgenciesAsync();
        await SeedMediaAsync();
        await SeedJobTypesAsync();
        await SeedClientsAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedMarketsAsync()
    {
        if (_context.Markets.Any())
            return;

        var markets = new[]
        {
            new Market { Name = "Ukraine", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await _context.Markets.AddRangeAsync(markets);
        _logger.LogInformation("Markets засіяно");
    }

    private async Task SeedAgenciesAsync()
    {
        if (_context.Agencies.Any())
            return;

        var agencies = new[]
        {
            new Agency { Name = "GroupM",    Country = "Ukraine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agency { Name = "MediaCom",  Country = "Ukraine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agency { Name = "Mindshare", Country = "Ukraine", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Agency { Name = "Wavemaker", Country = "Ukraine", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await _context.Agencies.AddRangeAsync(agencies);
        _logger.LogInformation("Agencies засіяно");
    }

    private async Task SeedContractingAgenciesAsync()
    {
        if (_context.ContractingAgencies.Any())
            return;

        var contractingAgencies = new[]
        {
            new ContractingAgency { Name = "Acceleration",        IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Choreograph",         IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Essence",             IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Finecast",            IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "GroupM",              IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "GroupM Services",     IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Keyade",              IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Kinetic",             IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "m/Six",               IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "MediaCom",            IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "MFuse",               IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Mindshare",           IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Motion Content Group",IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Wavemaker",           IsActive = true, CreatedAt = DateTime.UtcNow },
            new ContractingAgency { Name = "Xaxis",               IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await _context.ContractingAgencies.AddRangeAsync(contractingAgencies);
        _logger.LogInformation("ContractingAgencies засіяно");
    }

    private async Task SeedMediaAsync()
    {
        if (_context.Media.Any())
            return;

        var media = new[]
        {
            new Media { Name = "All media",           IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "OOH",                 IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Other",               IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Print",               IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Radio",               IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Research",            IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Trading",             IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "TV",                  IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "TVs",                 IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital - all",       IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital - Paid Social",  IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital - Paid Search",  IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital - Display",   IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital - Video",     IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital SP",          IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital Commerce",    IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital Influencers", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Media { Name = "Digital other",       IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await _context.Media.AddRangeAsync(media);
        _logger.LogInformation("Media засіяно");
    }

    private async Task SeedJobTypesAsync()
    {
        if (_context.JobTypes.Any())
            return;

        var jobTypes = new[]
        {
            new JobType { Name = "Strategy planning",  IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Media plans",        IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Campaign running",   IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Reporting",          IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Docs and finances",  IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Research",           IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Self education",     IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Vacation",           IsActive = true, CreatedAt = DateTime.UtcNow },
            new JobType { Name = "Other",              IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await _context.JobTypes.AddRangeAsync(jobTypes);
        _logger.LogInformation("JobTypes засіяно");
    }

    private async Task SeedClientsAsync()
    {
        if (_context.Clients.Any())
            return;

        var clientNames = new[]
        {
            "All clients", "NewBiz", "Adidas/Reebook", "Adobe", "Akzo Nobel",
            "Asahi", "Asatsu-DK (ADK)", "Audemars Piguet", "AXA", "Bayer",
            "Beiersdorf", "Beko", "Benetton", "BGL Group", "Blackrock",
            "Booking.com", "BP", "Bridgestone", "Bumble Holdings", "Calvin Klein",
            "Campari", "Cartier", "Chevron", "Coca-Cola", "Colgate-Palmolive",
            "Collistar", "Continental", "Danone", "Darnitsa", "Deloitte",
            "Deutsche Post DHL", "Deutsche Telekom", "Discovery", "DoorDash, Inc.", "Dr Oetker",
            "Duracell", "EA GAMES", "Edgewell - Global", "Energizer", "Erste Bank",
            "Essilor", "Falcon and Associates FZ", "Ford", "Formula 1", "Fozzy Group",
            "FrieslandCampina", "Garden Care Bidco Limited", "GE", "Geberit", "General Mills",
            "GoDaddy.com", "Haribo", "Hasbro", "Hawley & Hazel (H&H)", "HBO",
            "Helly Hansen", "Henkel", "Honor Global Master Purchase", "Huawei", "Husqvarna",
            "IAG (British Airways)", "IBM", "IHG", "IKEA", "Jaco",
            "Jetstar", "Johnson and Johnson", "Kapp-Ahl", "Karcher", "Kimberly-Clark",
            "Kingfisher", "Lavazza", "Lektravy", "Lombard Odier", "L'Oreal",
            "Lufthansa", "LVMH", "MARS", "McArthurGlen", "Menarini",
            "Mondelez", "MSC Cruises", "Nestlé", "Netflix", "Next",
            "NFL Gamepass (Overtier)", "Nike", "Osram", "P&G", "Paramount",
            "Pepsi", "Perfetti Van Melle", "Perrigo", "Pfizer", "Pripravka",
            "Recordati", "Rolex", "Royal Canin", "Savencia", "Shell",
            "Skechers", "Sony Playstation", "Subaru/Nissan", "Suntory", "Tata Global Beverages",
            "Tiffany", "Total Energy", "Toyota", "Trading", "Triumph",
            "Ubisoft", "UIP", "Unilever", "Versace", "Vodafone",
            "Volvo", "Weightwatchers", "Whirlpool", "Xerox", "Xiaomi",
            "Yum!", "Indeed", "Innocent", "Vacheron", "Klarna",
            "Breuninger", "CCHBC"
        };

        var clients = clientNames
            .Select(name => new Client
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToArray();

        await _context.Clients.AddRangeAsync(clients);
        _logger.LogInformation("Clients засіяно ({Count} записів)", clients.Length);
    }
}
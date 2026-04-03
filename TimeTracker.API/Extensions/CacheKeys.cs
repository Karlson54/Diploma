namespace TimeTracker.API.Extensions;

public static class CacheKeys
{
    public const string AgenciesAll = "agencies_all";
    public const string AgenciesActive = "agencies_active";
    public const string MarketsAll = "markets_all";
    public const string MarketsActive = "markets_active";
    public const string ClientsAll = "clients_all";
    public const string ClientsActive = "clients_active";
    public const string MediaAll = "media_all";
    public const string MediaActive = "media_active";
    public const string JobTypesAll = "jobtypes_all";
    public const string JobTypesActive = "jobtypes_active";
    public const string ContractingAgenciesAll = "contractingagencies_all";
    public const string ContractingAgenciesActive = "contractingagencies_active";

    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);
}
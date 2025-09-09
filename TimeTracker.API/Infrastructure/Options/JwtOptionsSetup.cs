// TimeTracker.API/Infrastructure/Options/JwtOptionsSetup.cs
using Microsoft.Extensions.Options;
using TimeTracker.Core.Common;

namespace TimeTracker.API.Infrastructure.Options;

public class JwtOptionsSetup : IConfigureOptions<JwtSettings>
{
    private const string SectionName = "JwtSettings";
    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtSettings options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}
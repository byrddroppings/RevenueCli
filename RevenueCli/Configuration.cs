using Microsoft.Extensions.Configuration;
using Serilog;

namespace RevenueCli;

public record Configuration(
    string SalesforceLoginUrl,
    string SalesforceClientId,
    string SalesforceClientSecret,
    string SalesforceRefreshToken)
{
    private static readonly Dictionary<string, string> AzureAppConfig = new()
    {
        { "staging", "Endpoint=https://dxp-stg-config.azconfig.io;Id=kSod-l5-s0:g8tLxqfLC8zR+98YXSCj;Secret=vjQUzwZbkzoC0UWjFEbwDOVNR6hHLDkjY4WnbxLxPpk="},
        { "production", "" },
    };
    
    public static Configuration ForEnvironment(string environment)
    {
        var config = new ConfigurationBuilder()
            .AddAzureAppConfiguration(AzureAppConfig[environment])
            .Build();

        return new Configuration(
            $"{config["Salesforce:BaseUrl"]}/services/oauth2/token",
            config["Salesforce:Authentication:ClientId"],
            config["Salesforce:Authentication:ClientSecret"],
            config["Salesforce:Authentication:RefreshToken"]
        );
    }
};
using NetCoreForce.Client;

namespace RevenueCli;

public static class SalesforceClient
{
    public static async Task<ForceClient> ForEnvironment(string environment)
    {
        var config = Configuration.ForEnvironment(environment);

        var auth = new AuthenticationClient();

        await auth.TokenRefreshAsync(
            config.SalesforceRefreshToken,
            config.SalesforceClientId,
            config.SalesforceClientSecret,
            config.SalesforceLoginUrl);

        return new ForceClient(auth.AccessInfo.InstanceUrl, auth.ApiVersion, auth.AccessInfo.AccessToken);
    }
}
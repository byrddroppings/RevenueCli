using System.ComponentModel;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Serilog;

namespace RevenueCli;

public class RevenueCommands
{
    public async Task Update(string environment, int year, int month)
    {
        Log.Information("Command: {Command}", "revenue copy");
        Log.Information("Environment: {Environment}", environment);
        Log.Information("Year: {Year}", year);
        Log.Information("Month: {Month}", month);

        var client = await SalesforceClient.ForEnvironment(environment);
        var period = FormatPeriod(year, month);
        await using var db = new SqliteConnection("DataSource=:memory:");

        var soql = $@"select Year__c,
                        Month__c,
                        EngagementNumber__c,
                        AmountUSDNotAdjusted__c,
                        AmountNotAdjusted__c,
                        DiscountUSD__c,
                        Discount__c 
                      from Revenue__c
                      where Period__c = '{period}'";
        
        var revenues = await client.Query<Revenue>(soql);
        Log.Debug("got {Count:N0} records", revenues.Count);
    }

    private static string FormatPeriod(int year, int month)
        => $"{year:D4}{month:D2}";

    private record Revenue(
        [JsonProperty("Year__c")] int RevenueYear,
        [JsonProperty("Month__c")] int RevenueMonth,
        [JsonProperty("EngagementNumber__c")] string EngagementId,
        [JsonProperty("AmountUSDNotAdjusted__c")] decimal Amount,
        [JsonProperty("AmountNotAdjusted__c")] decimal LocalAmount,
        [JsonProperty("DiscountUSD__c")] decimal Discount,
        [JsonProperty("Discount__c")] decimal DiscountLocal
    );
}
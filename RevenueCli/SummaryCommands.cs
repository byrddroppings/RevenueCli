using NetCoreForce.Client;
using Newtonsoft.Json;
using Serilog;

namespace RevenueCli;

public class SummaryCommands
{
    public async Task Verify(string environment, int year, int month)
    {
        Log.Information("Command: {Command}", "summary verify");
        Log.Information("Environment: {Environment}", environment);
        Log.Information("Year: {Year}", year);
        Log.Information("Month: {Month}", month);

        var client = await SalesforceClient.ForEnvironment(environment);
        var period = FormatPeriod(year, month);

        var summarySoql = $@"select Period__c, Count__c, Amount__c, IsComplete__c, LastUpdated__c
                      from MonthlyRevenueSummary__c where Period__c = '{period}'";
        
        var summary = await client.QuerySingle<dynamic>(summarySoql);

        if (summary == null)
        {
            Log.Error("Monthly Revenue Summary not found for Period: {Period}", period);
            return;
        }

        var (expectedTotal, expectedCount) = await GetRevenues(client, period);
        var isError = false;

        if (expectedTotal != (decimal)summary.Amount__c)
        {
            Log.Error("Summary.Amount__c = {Amount}, ExpectedTotal = {ExpectedTotal}", summary.Amount__c, expectedTotal);
            isError = true;
        }

        if (expectedCount != (decimal)summary.Count__c)
        {
            Log.Error("Summary.Count__c = {Count}, ExpectedCount = {ExpectedCount}", summary.Count__c, expectedCount);
            isError = true;
        }

        if (!(bool)summary.IsComplete__c)
        {
            Log.Error("Summary is not marked as complete");
            isError = true;
        }

        if (!isError)
        {
            Log.Debug("no errors found!");
        }
    }

    public async Task Repair(string environment, int year, int month)
    {
        Log.Information("Command: {Command}", "summary correct");
        Log.Information("Environment: {Environment}", environment);
        Log.Information("Year: {Year}", year);
        Log.Information("Month: {Month}", month);

        var client = await SalesforceClient.ForEnvironment(environment);
        var period = FormatPeriod(year, month);
        var (expectedTotal, expectedCount) = await GetRevenues(client, period);
        
        var summary = new
        {
            Amount__c = expectedTotal,
            Count__c = expectedCount,
            IsComplete__c = true,
            LastUpdated__c = DateTimeOffset.UtcNow,
        };

        await client.InsertOrUpdateRecord("MonthlyRevenueSummary__c", "Period__c", period, summary);
        
        Log.Information("{Summary}", Serialize(summary));
    }

    private async Task<(decimal expectedTotal, int expectedCount)> GetRevenues(ForceClient client, string period)
    {
        var revenueSoql = $@"select UniqueKey__c, Period__c, EngagementNumber__c, AmountUSDNotAdjusted__c
                             from Revenue__c where Period__c = '{period}'";
        var revenues = await client.Query<dynamic>(revenueSoql);

        var expectedTotal = revenues.Sum(r => r.AmountUSDNotAdjusted__c);
        var expectedCount = revenues.Count;

        return (expectedTotal, expectedCount);
    }
    
    private static string FormatPeriod(int year, int month)
        => $"{year:D4}{month:D2}";

    private static string Serialize<T>(T obj)
        => JsonConvert.SerializeObject(obj, Formatting.Indented);
}
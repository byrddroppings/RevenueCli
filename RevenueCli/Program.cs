using Cocona;
using RevenueCli;
using Serilog;
using Serilog.Core;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();
    
var app = CoconaLiteApp.Create();
app.AddSubCommand("summary", c => c.AddCommands<SummaryCommands>());
app.AddSubCommand("revenue", c => c.AddCommands<RevenueCommands>());
await app.RunAsync();



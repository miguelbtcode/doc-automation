var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql").WithLifetime(ContainerLifetime.Persistent).WithDataVolume();

var docAutomationDb = sql.AddDatabase("DocAutomation");

builder
    .AddProject<Projects.DocAutomation_Web>("web")
    .WithReference(docAutomationDb)
    .WaitFor(docAutomationDb);

builder.Build().Run();

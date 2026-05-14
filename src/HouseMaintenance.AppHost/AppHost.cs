var builder = DistributedApplication.CreateBuilder(args);

var openAiApiKey = builder.AddParameter("openAiApiKey", secret: true);

var api = builder.AddProject<Projects.HouseMaintenance_API>("housemaintenance-api")
    .WithEnvironment("OpenAI__ApiKey", openAiApiKey)
    .WithEnvironment("OpenAI__Model", "gpt-4o-mini")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.HouseMaintenance_Web>("housemaintenance-web")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();

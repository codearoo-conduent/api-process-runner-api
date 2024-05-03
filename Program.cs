using api_process_runner_api.Helpers;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string _apiDeploymentName = Helper.GetEnvironmentVariable("ApiDeploymentName");
string _apiEndpoint = Helper.GetEnvironmentVariable("ApiEndpoint");
string _apiKey = Helper.GetEnvironmentVariable("ApiKey");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the Semantic Kernel as a transient service
builder.Services.AddTransient<Kernel>(s =>
{
    var builder = Kernel.CreateBuilder();
    builder.AddAzureOpenAIChatCompletion(
        _apiDeploymentName,
        _apiEndpoint,
        _apiKey
    );
    return builder.Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

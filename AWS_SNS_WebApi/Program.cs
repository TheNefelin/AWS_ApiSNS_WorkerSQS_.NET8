using Amazon;
using Amazon.SimpleNotificationService;
using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var AWS_REGION = builder.Configuration["AWS_REGION"] ?? "us-east-1";
var SNS_TOPIC_ARN = builder.Configuration["SNS_TOPIC_ARN"] ?? "arnarn:aws:sns:us-east-1:123:my-sns";

builder.Services.AddSingleton(new SnsConfig { SNS_TOPIC_ARN = SNS_TOPIC_ARN });
builder.Services.AddSingleton<SnsService>();
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp => {
    return new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(AWS_REGION));
});

var DB_HOST = builder.Configuration["DB_HOST"] ?? "localhost";
var DB_PORT = builder.Configuration["DB_PORT"] ?? "5432";
var DB_NAME = builder.Configuration["DB_NAME"] ?? "postgres";
var DB_USER = builder.Configuration["DB_USER"] ?? "postgres";
//var DB_NAME = builder.Configuration["DB_NAME"] ?? "db_testing";
//var DB_USER = builder.Configuration["DB_USER"] ?? "testing";
var DB_PASSWORD = builder.Configuration["DB_PASSWORD"] ?? "testing";

var _connectionString = $"Host={DB_HOST};Port={DB_PORT};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(_connectionString);
    //options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
var logConfig = new LoggerConfiguration();
logConfig.ReadFrom.Configuration(builder.Configuration);
var logger = logConfig.Enrich.FromLogContext().CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSerilog();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(); // Access via http://localhost:<port>/swagger
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseHealthChecks("/");

app.UseSerilogRequestLogging();

app.Run();

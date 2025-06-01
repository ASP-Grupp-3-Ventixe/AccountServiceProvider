using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Presentation.Data;
using Presentation.Services;
using Swashbuckle.AspNetCore.Filters;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Account Service API Documentation",
        Description = "Official Documentation for Account Service Provider API."
    });
    o.EnableAnnotations();
    o.ExampleFilters(); 
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();


builder.Services.AddGrpc();
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(x =>
{
    x.SignIn.RequireConfirmedEmail = true;
    x.User.RequireUniqueEmail = true;
    x.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddSingleton<AccountServiceBusHandler>();

var app = builder.Build();
app.MapOpenApi();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ventixe AccountServiceProvider");
    c.RoutePrefix = string.Empty;
});

app.UseGrpcWeb();
app.MapGrpcService<AccountService>().EnableGrpcWeb(); 
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();

using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SecureApi.Constants;
using System.Text;
using AspNetCoreRateLimit;
using SecureApi.StartupConfig;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCaching();

builder.Services.AddMemoryCache();
builder.AddRateLimitServices();

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy(PolicyConstants.Admin, policy =>
    {
        policy.RequireClaim("Role", "admin");
    });
    opts.AddPolicy(PolicyConstants.Teacher, policy =>
    {
        policy.RequireClaim("Role", "teacher");
        policy.RequireClaim("Permission", "read", "write");
    });
    opts.AddPolicy(PolicyConstants.Student, policy =>
    {
        policy.RequireClaim("Role", "student");
        policy.RequireClaim("Permission", "read", "write");
    });
    // opts.FallbackPolicy = new AuthorizationPolicyBuilder()
    //     .RequireAuthenticatedUser()
    //     .Build();
});
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                builder.Configuration.GetValue<string>("Authentication:SecretKey")))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseIpRateLimiting();

app.Run();

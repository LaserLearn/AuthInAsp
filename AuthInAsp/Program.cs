using AuthenticationInCleanArchitecture.Persistence.Repositories;
using AuthInAsp;
using AuthInAsp.Contracts.Generic;
using AuthInAsp.IdentityService.Abstract;
using AuthInAsp.IdentityService.Implement;
using AuthInAsp.Repositories.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();



#region Swagger and Identity
//add
builder.Services.AddHttpContextAccessor();

//add
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "LaserLearn API",
        Description = "A simple API for authentication and user management"
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});


//add
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });





builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://LaserLearn.ir")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


#endregion

#region DB
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");

builder.Services.AddDbContext<Context_En>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


builder.Services.AddScoped<Iuser, User_Rep>();

#endregion


// DI
builder.Services.AddScoped<IUserService, UserService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LaserLearn API v1");
    });
}

//add
app.UseCors("AllowedOrigins");


app.UseHttpsRedirection();

//add
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

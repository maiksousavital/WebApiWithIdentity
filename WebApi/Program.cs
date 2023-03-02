using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository;
using Repository.Models;
using Service.Interfaces;
using Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<Context>(opts =>
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<Context>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IEmailService, EmailService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();

using HotelManagementService.Data;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Repositories.Implementations;
using HotelManagementService.Services.Interfaces;
using HotelManagementService.Services.Implementations;
using Microsoft.EntityFrameworkCore;

// Prevent inotify limit exceptions on Linux environments
Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE", "false");
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("HotelDb")));

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

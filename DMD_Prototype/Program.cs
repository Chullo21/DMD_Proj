﻿using DMD_Prototype.Controllers;
using DMD_Prototype.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISharedFunct, UniversalFunctions>();

builder.Services.AddControllersWithViews();

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ATS}/{action=ATSMenu}/{id?}");

app.Run();

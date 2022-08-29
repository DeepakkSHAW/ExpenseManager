using ExpenseManager.API;
using ExpenseManager.API.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using UserManagement.Lib;
using UserManagement.Lib.Models;


//private readonly Users userA = new Users(UserType.Admin, "deepak.shaw@gmail.com") { Name = "Deepak", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now), Password = "PassWord@123", PhoneNumber = "046-001-2002" };
//private readonly Users userB = new Users(UserType.Admin, "rupam.shaw@gmail.com") { Name = "Rupam", Surname = "Shaw", DOB = DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), Password = "PassWord@ABC", PhoneNumber = "001-002-3003" };


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(o =>
//    //o.UseInlineDefinitionsForEnums();
//    //o.SchemaFilter<EnumSchemaFilter>();
//    //o.DescribeAllEnumsAsStrings()
//o.SwaggerDoc("v1", new() { Title = "Expense Manager API", Version = "v 1.0" })
//);
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "Version 1.0",
        Title = "Expense Manager API",
        Description = "A PoC using .Net6 with minimal API to illustrate User Management and Expense Management",
        TermsOfService = new Uri("https://msdn/terms"),
        Contact = new OpenApiContact
        {
            Name = "Deepak Shaw",
            Email = "DK@shaw.com",
            Url = new Uri("https://twitter.com/deepak.shaw"),
        },
        License = new OpenApiLicense
        {
            Name = "Free License term",
            Url = new Uri("https://msdn/terms"),
        }
    });
    //options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<EnumSchemaFilter>();
});

//builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<UserManagement.Lib.Models.UserDto>());
builder.Services.AddScoped<IValidator<UserDto>, UserValidation>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
//builder.Configuration.AddIniFile("appsettings.ini");
builder.Configuration.AddJsonFile("appsettings.json");
var app = builder.Build();
app.UseSwagger();

#region For quick unit testing of http endpoint (can be cleaned up after development
//app.MapGet("/", () => "---------------Expense Manager Console---------------");

//app.MapGet("/api/testConfig", [AllowAnonymous] ([FromServices] IAuthorRepository custRepo) =>
//{
//    //return Results.Ok(custRepo.GetAuthor(1));
//    return Results.Ok(custRepo.GetConfigurationInfo());
//});
//app.MapGet("/api/testRepo/{id}", [AllowAnonymous] ([FromServices] IAuthorRepository custRepo, int id) =>
//{
//    return Results.Ok(custRepo.GetAuthor(id));
//});
#endregion

#region User Management API's
app.MapPost("/api/new-user", [AllowAnonymous] async ([FromServices] UserService userService, IValidator<UserDto> validator, UserDto user) =>
{
    var validationResult = validator.Validate(user);
    if (!validationResult.IsValid)
    {
        var error = new { error = validationResult.Errors.Select(x => x.ErrorMessage) };
        return Results.BadRequest(error);
    }
    var optResult = await userService.NewUserAsync(user);
    return Results.Created($"/api/customers/{((Users)optResult.returnObject).RowKey}", user);
});

app.MapGet("/api/get-user/{email}", [AllowAnonymous] async ([FromServices] UserService userService, IValidator<UserDto> validator, string email) =>
{
    if (!(new EmailAddressAttribute().IsValid(email)))
    {
        return Results.BadRequest(new { Errror = "Invalid email id provided." });
    }

    var optResult = await userService.GetUserDetailsAsync(email);
    if (optResult.status)
    {
        UserDto apiUser = new UserDto();
        Users users = (Users)optResult.returnObject;

        return Results.Ok(new UserDto
        {
            Name = users.Name,
            Surname = users.Surname,
            DOB = users.DOB,
            PhoneNumber = users.PhoneNumber,
            Password = "xxxxxxxx",
            EmailID = users.RowKey
        });
    }
    else
    {
        return Results.Problem();
    }
});

app.MapDelete("/api/delete-user/{email}", [AllowAnonymous] async ([FromServices] UserService userService, IValidator<UserDto> validator, string email) =>
{
    if (!(new EmailAddressAttribute().IsValid(email)))
    {
        return Results.BadRequest(new { Errror = "Invalid email id provided." });
    }

    var optResult = await userService.DeleteUserAsync(email);
    if (optResult.status)
    {
        return Results.NoContent();
    }
    else
    {
        return Results.Problem();
    }
});

app.MapPut("/api/update-user/{email}", [AllowAnonymous] async ([FromServices] UserService userService, IValidator<UserDto> validator, string email, UserDto user) =>
{
    var validationResult = validator.Validate(user);
    if (!validationResult.IsValid)
    {
        var error = new { error = validationResult.Errors.Select(x => x.ErrorMessage) };
        return Results.BadRequest(error);
    }
    if (!(new EmailAddressAttribute().IsValid(email)))
    {
        return Results.BadRequest(new { Errror = "Invalid email id provided." });
    }
    var optResult = await userService.UpdateUserAsync(user, email);
    return Results.Created($"/api/customers/{((Users)optResult.returnObject).RowKey}", user);
});
#endregion
app.UseSwaggerUI();
app.Run();

using Contacts.Server.Repositories;
using Contacts.Shared;
using EdgeDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
        c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudience = builder.Configuration["Auth0:Audience"],
            ValidIssuer = $"https://{builder.Configuration["Auth0:Domain"]}"
        };
    });
// Add services to the container.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CreateAccess", policy =>
                          policy.RequireClaim("permissions", "create:contact"));
    options.AddPolicy("UpdateAccess", policy =>
                      policy.RequireClaim("permissions", "update:contact"));
    options.AddPolicy("DeleteAccess", policy =>
                      policy.RequireClaim("permissions", "delete:contact"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEdgeDB(EdgeDBConnection.FromInstanceName("Contacts"), config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});
builder.Services.AddScoped<IContactRepository, ContactRepository>();





var app = builder.Build();






// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapPost("/new-contact", async ([FromBody] Contact newContact, IContactRepository _contactRepository) =>
{
    try
    {
        await _contactRepository.Create(newContact);
        return Results.Ok();
    }
    catch (Exception)
    {
        return Results.BadRequest("Error inserting data into the database");
    }
}).RequireAuthorization("CreateAccess");

app.MapGet("/contacts", async (IContactRepository _contactRepository) =>
{
    try
    {
        var contacts = await _contactRepository.GetAll();
        return Results.Ok(contacts);
    }
    catch (Exception)
    {
        return Results.BadRequest("Error retrieving data from the database");
    }
}).RequireAuthorization();

app.MapGet("/contacts/{id}", async (string id, IContactRepository _contactRepository) =>
{
    try
    {
        Guid guidId = Guid.Parse(id);
        var contact = await _contactRepository.GetById(guidId);
        if (contact is null)
        {
            return Results.NotFound();
        }
        return Results.Ok(contact);
    }
    catch (Exception)
    {
        return Results.BadRequest("Error retrieving data from the database");
    }
}).RequireAuthorization();

app.MapDelete("/delete-contact/{id}", async (string id, IContactRepository _contactRepository) =>
{
    try
    {
        Guid guidId = Guid.Parse(id);
        if (string.IsNullOrEmpty(id))
        {
            return Results.BadRequest();
        }
        await _contactRepository.Delete(guidId);
        return Results.Ok();
    }
    catch (Exception)
    {
        return Results.BadRequest("Error Deleting from database");
    }
}).RequireAuthorization("DeleteAccess");

app.MapGet("/search-contact/{search}", async (string search, IContactRepository _contactRepository) =>
{
    try
    {
        var contacts = await _contactRepository.Search(search);
        return Results.Ok(contacts);
    }
    catch (Exception)
    {
        return Results.BadRequest("Error retrieving search results");
    }
}).RequireAuthorization();

app.MapPut("/edit-contact", async ([FromBody] Contact editContact, IContactRepository _contactRepository) =>
{
    try
    {
        await _contactRepository.Update(editContact);
        return Results.Ok();
    }
    catch (Exception)
    {
        return Results.BadRequest("Error updating data in the database");
    }
}).RequireAuthorization("UpdateAccess");

app.Run();

using Contacts.Server.Repositories;
using Contacts.Shared;
using EdgeDB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEdgeDB(EdgeDBConnection.FromInstanceName("Contacts"), config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
var app = builder.Build();

app.MapPost("/new-contact", async ([FromBody] Contact newContact, IContactRepository _contactRepository, IPasswordHasher _passwordHasher) =>
{
    try
    {
        newContact.Password = _passwordHasher.HashPassword(newContact.Password);
        await _contactRepository.Create(newContact);
        return Results.Ok();
    }
    catch (Exception)
    {
        return Results.BadRequest("Error inserting data into the database");
    }
});

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
});

app.MapGet("/contacts/{id}", async (string id, IContactRepository _contactRepository) =>
{
    try
    {
        Guid guidId = Guid.Parse(id);
        var contact = await _contactRepository.GetById(guidId);
        if(contact is null)
        {
            return Results.NotFound();
        }
        return Results.Ok(contact);
    }
    catch (Exception)
    {
        return Results.BadRequest("Error retrieving data from the database");
    }
});

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
});

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
});

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
});

app.MapGet("/validate-sign-in", async (HttpContext context, IContactRepository _contactRepository) =>
{
    Contact newContact = await _contactRepository.AuthenticateUser(context.Request.Query["username"], context.Request.Query["password"]);
    if(newContact is not null)
    {
        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, newContact.Username),
                    new Claim(ClaimTypes.Role, newContact.Roles.ToString())
                };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        return Results.Ok();
    }
    else
    {
        return Results.BadRequest("Invalid username or password");
    }
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.RedirectToRoute("/SignIn");
});

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

app.Run();

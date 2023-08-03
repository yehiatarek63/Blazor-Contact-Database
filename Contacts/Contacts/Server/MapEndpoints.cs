using Contacts.Server.Repositories;
using Contacts.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.Server;

public static class MapEndpoints
{
    public static RouteGroupBuilder MapContactsApi(this RouteGroupBuilder group)
    {
        group.MapGet("contacts", async (IContactRepository _contactRepository) =>
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

        group.MapGet("contacts/{id}", async (string id, IContactRepository _contactRepository) =>
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
        });

        group.MapGet("search-contact/{search}", async (string search, IContactRepository _contactRepository) =>
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
        return group;
    }

    public static RouteGroupBuilder MapAuthorizedContactsApi(this RouteGroupBuilder group)
    {
        group.MapPost("/new-contact", async ([FromBody] Contact newContact, IContactRepository _contactRepository) =>
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


        group.MapDelete("/delete-contact/{id}", async (string id, IContactRepository _contactRepository) =>
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


        group.MapPut("/edit-contact", async ([FromBody] Contact editContact, IContactRepository _contactRepository) =>
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


        return group;
    }
}

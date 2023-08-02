using Contacts.Shared;
using EdgeDB;
using Microsoft.AspNet.Identity;

namespace Contacts.Server.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly EdgeDBClient _client;
    public ContactRepository(EdgeDBClient client)
    {
        _client = client;
    }
    public async Task Create(Contact contact)
    {
        var query = "INSERT Contact {first_name := <str>$first_name, last_name := <str>$last_name, email := <str>$email, title := <State>$title, description := <str>$description, date_of_birth := <datetime>$date_of_birth, marriage_status := <bool>$marriage_status}";
        await _client.ExecuteAsync(query, new Dictionary<string, object?>
        {
                    {"first_name", contact.FirstName},
                    {"last_name", contact.LastName},
                    {"email", contact.Email},
                    {"title", contact.Title},
                    {"description", contact.Description},
                    {"date_of_birth", contact.DateOfBirth},
                    {"marriage_status", contact.MarriageStatus}
        });
    }

    public async Task Delete(Guid id)
    {
        await _client.ExecuteAsync("DELETE Contact FILTER .id = <uuid>$id", new Dictionary<string, object?>
        {
            { "id", id }
        });
    }

    public async Task<List<Contact>> GetAll()
    {
        return (await _client.QueryAsync<Contact>("SELECT Contact {*}")).ToList();
    }

    public async Task<Contact> GetById(Guid id)
    {
        return await _client.QuerySingleAsync<Contact>("SELECT Contact{*} FILTER .id = <uuid>$id", new Dictionary<string, object?> { { "id", id } });
    }

    public async Task<List<Contact>> Search(string search)
    {
        var searchQuery = "SELECT Contact {*} FILTER .first_name ILIKE '%' ++ <str>$first_name ++ '%' OR .last_name ILIKE '%' ++ <str>$last_name ++ '%' OR .email ILIKE '%' ++ <str>$email ++ '%'";
        return (await _client.QueryAsync<Contact>(searchQuery, new Dictionary<string, object?>
                {
                    { "first_name", search},
                    { "last_name", search },
                    { "email", search }
                })).ToList();
    }

    public async Task Update(Contact editContact)
    {
        var query = "UPDATE Contact FILTER .id = <uuid>$id SET {first_name := <str>$first_name, last_name := <str>$last_name, email := <str>$email, title := <State>$title, description := <str>$description, date_of_birth := <datetime>$date_of_birth, marriage_status := <bool>$marriage_status}";
        await _client.ExecuteAsync(query, new Dictionary<string, object?>
                {
                    {"id", editContact.Id},
                    {"first_name", editContact.FirstName},
                    {"last_name", editContact.LastName},
                    {"email", editContact.Email},
                    {"title", editContact.Title},
                    {"description", editContact.Description},
                    {"date_of_birth", editContact.DateOfBirth},
                    {"marriage_status", editContact.MarriageStatus}
                });
    }
}


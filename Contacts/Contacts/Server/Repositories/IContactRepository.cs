using Contacts.Shared;

namespace Contacts.Server.Repositories;

public interface IContactRepository
{
    Task<List<Contact>> GetAll();
    Task<Contact> GetById(Guid id);
    Task Create(Contact contact);
    Task Update(Contact editContact);
    Task Delete(Guid id);
    Task<List<Contact>> Search(string search);
    Task<Contact?> AuthenticateUser(string username, string password);
}

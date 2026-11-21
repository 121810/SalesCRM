using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IClientService
    {
        Task CreateClient(CreateClientViewModel model);
        Task UpdateClient(Guid id, CreateClientViewModel model);
        Task SoftDeleteClient(Guid id);
        Task<List<ClientMaster>> GetAllClients(ClientFilterViewModel? filter = null);
        Task<List<string>> GetDistinctAreasAsync();
        Task<ClientMaster?> GetSingleClientDetails(Guid id);
        Task<ClientDetailsViewModel?> GetClientDetailsWithVisits(Guid id);
    }
}

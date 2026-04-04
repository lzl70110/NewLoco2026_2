using NewLoco.Web.ViewModels.Axles;

namespace NewLoco.Service.Core.Contracts
{
    public interface IAxleMeasurementService
    {
        Task<List<AxleMeasurementCardListViewModel>> GetAllAsync();
        Task<AxleMeasurementCardDetailsViewModel> GetDetailsAsync(int id);

        Task<AxleMeasurementCardViewModel> GetCreateModelAsync();
        Task<int> CreateAsync(AxleMeasurementCardViewModel model, string createdBy);

        Task<AxleMeasurementCardViewModel> GetEditModelAsync(int id);
        Task UpdateAsync(AxleMeasurementCardViewModel model, string modifiedBy);

        // Business logic helper
        void CalculateSr(AxleMeasurementCardViewModel model);

        // AJAX helper
        Task<int> GetAxlesCountAsync(int locoId);
    }
}
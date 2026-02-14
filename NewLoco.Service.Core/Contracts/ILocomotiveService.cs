using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Service.Core.Contracts;

public interface ILocomotiveService
    {
    Task<IEnumerable<LocomotiveNumberViewModel>> GetAll(string filter);

    Task<LocomotiveDetailsViewModel?> GetDetails(int id);

    Task CreateAsync(LocomotiveFormModel model, string user);

    Task<LocomotiveFormModel?> GetForEdit(int id);

    Task EditAsync(int id, LocomotiveFormModel model, string user);

    Task DeleteAsync(int id, string user);

    Task UndeleteAsync(int id, string user);
    }

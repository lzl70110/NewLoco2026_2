using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Service.Core.Contracts;

public interface IFuelService
    {
    IEnumerable<FuelAllViewModel> GetAll();

    CreateFuelViewModel CreateModel();

    Task CreateAsync(CreateFuelViewModel model, string user);

    FuelAllViewModel? GetForEdit(int id);

    Task EditAsync(int id, FuelAllViewModel model, string user);

    Task DeleteAsync(int id, string user);

    Task UndoDeleteAsync(int id, string user);

    decimal GetLastFuel(int locomotiveId);
    }

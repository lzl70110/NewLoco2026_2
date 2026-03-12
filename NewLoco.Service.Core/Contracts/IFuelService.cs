using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Service.Core.Contracts
{
    public interface IFuelService
    {
        // full dataset (used by FuelReport)
        IEnumerable<FuelAllViewModel> GetAll();

        // slim dataset (used by Fuels/Index)
        IEnumerable<FuelsBasicDetailsViewModel> GetForIndexLatest();

        CreateFuelViewModel CreateModel();

        Task CreateAsync(CreateFuelViewModel model, string user);

        // consume from the latest record for the locomotive
        Task ConsumeFuelAsync(int locomotiveId, decimal amount, string user);

        // full details/edit VM
        FuelAllViewModel? GetForEdit(int id);

        Task EditAsync(int id, FuelAllViewModel model, string user);

        Task DeleteAsync(int id, string user);

        Task UndoDeleteAsync(int id, string user);

        decimal GetLastFuel(int locomotiveId);

        Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewLoco.GCommon.Enums;                 // Shift
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Service.Core.Contracts
{
    public interface IFuelService
    {
        // --------------------------
        // Legacy (kept for UI wiring)
        // --------------------------
        IEnumerable<FuelAllViewModel> GetAll();
        IEnumerable<FuelsBasicDetailsViewModel> GetForIndexLatest();
        CreateFuelViewModel CreateModel();

        // legacy create (daily row) – kept for existing forms
        Task CreateAsync(CreateFuelViewModel model, string user);

        // legacy consume (decimal) – forwards to the int version with depot step
        Task ConsumeFuelAsync(int locomotiveId, decimal amount, string user);

        FuelAllViewModel? GetForEdit(int id);
        Task EditAsync(int id, FuelAllViewModel model, string user);
        Task DeleteAsync(int id, string user);
        Task UndoDeleteAsync(int id, string user);

        // helpers/queries
        decimal GetLastFuel(int locomotiveId);
        Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date);
        Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date, Shift shift, CancellationToken ct = default);

        // --------------------------------
        // New, clean, domain-oriented API
        // --------------------------------

        /// <summary>Returns current fuel stock (latest FinalFuel) for a locomotive.</summary>
        Task<decimal> GetCurrentStockAsync(int locomotiveId, CancellationToken ct = default);

        /// <summary>Registers a refuel (IN). Amount is in liters and must respect the depot step.</summary>
        Task RefuelAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default);

        /// <summary>Consumes fuel (OUT) for TODAY (Shift.Day if not otherwise stated). If no row for today exists, a new daily row will be created.</summary>
        Task ConsumeAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default);

        /// <summary>Consumes fuel (OUT) on a specific date and shift. If the row does not exist, it will be created.</summary>
        Task ConsumeOnAsync(int locomotiveId, DateTime date, Shift shift, int liters, string user, string? note = null, CancellationToken ct = default);
    }
}
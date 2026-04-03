using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewLoco.GCommon.Enums;
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Service.Core.Contracts
{
    public interface IFuelService
    {
        // -----------------------------------------------------
        // Legacy API (kept for UI compatibility)
        // -----------------------------------------------------
        IEnumerable<FuelAllViewModel> GetAll();
        IEnumerable<FuelsBasicDetailsViewModel> GetForIndexLatest();
        CreateFuelViewModel CreateModel();

        Task CreateAsync(CreateFuelViewModel model, string user);
        Task ConsumeFuelAsync(int locomotiveId, decimal amount, string user);

        FuelAllViewModel? GetForEdit(int id);
        Task EditAsync(int id, FuelAllViewModel model, string user);

        Task DeleteAsync(int id, string user);
        Task UndoDeleteAsync(int id, string user);

        decimal GetLastFuel(int locomotiveId);
        Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date);
        Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date, Shift shift, CancellationToken ct = default);

        // -----------------------------------------------------
        // New domain-oriented API
        // -----------------------------------------------------

        /// <summary>
        /// Returns the latest FinalFuel value (current stock).
        /// </summary>
        Task<decimal> GetCurrentStockAsync(int locomotiveId, CancellationToken ct = default);

        /// <summary>
        /// Registers refueling (IN). Must match depot step rules.
        /// </summary>
        Task RefuelAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default);

        /// <summary>
        /// Consumes fuel for today, Shift.Day. Creates row if missing.
        /// </summary>
        Task ConsumeAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default);

        /// <summary>
        /// Consumes fuel on a specific date and shift. Creates row if missing.
        /// </summary>
        Task ConsumeOnAsync(
            int locomotiveId,
            DateTime date,
            Shift shift,
            int liters,
            string user,
            string? note = null,
            CancellationToken ct = default);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewLoco.Service.Core.Contracts;          // ShiftWorkDto, ShiftWorkQuery live here
using NewLoco.Web.ViewModels.ShiftWorks;       // Create/Edit view models (service accepts Web VMs)

namespace NewLoco.Service.Core.Contracts
{
    public interface IShiftWorkService
    {
        // Factory for empty Create VM
        CreateShiftWorkViewModel CreateModel();

        // Search + paging
        Task<(IEnumerable<ShiftWorkDto> Items, int Total)> GetAllAsync(ShiftWorkQuery query, CancellationToken ct = default);

        // Helpers
        Task<ShiftWorkDto?> GetLastShiftAsync(int locomotiveId);

        // Edit flow (sync signature per your controller)
        EditShiftWorkViewModel? GetForEdit(int id);

        // Commands
        Task CreateAsync(CreateShiftWorkViewModel model, string user);
        Task EditAsync(int id, EditShiftWorkViewModel model, string user);
        Task DeleteAsync(int id, string user);
        Task UndoDeleteAsync(int id, string user);
    }
}
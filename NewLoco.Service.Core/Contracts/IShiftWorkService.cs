using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewLoco.GCommon.Enums;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Service.Core.Contracts
{
    public interface IShiftWorkService
    {
        // legacy list used elsewhere in the app
        IEnumerable<ShiftWorksViewModel> GetAll();

        // create/edit flow
        CreateShiftWorkViewModel CreateModel();
        Task<ShiftWorkDto?> GetLastShiftAsync(int locomotiveId);
        Task CreateAsync(CreateShiftWorkViewModel model, string user);
        EditShiftWorkViewModel? GetForEdit(int id);
        Task EditAsync(int id, EditShiftWorkViewModel model, string user);
        Task DeleteAsync(int id, string user);
        Task UndoDeleteAsync(int id, string user);

        Task<bool> ExistsAsync(int locomotiveId, DateTime date, Shift shift, int? excludeId = null);

        // list + paging endpoint used by Shift Work search UI
        Task<(IReadOnlyList<ShiftWorkDto> Items, int TotalCount)>
            GetAllAsync(ShiftWorkQuery query, CancellationToken ct = default);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewLoco.GCommon.Enums;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Service.Core.Contracts
    {
    public interface IShiftWorkService
        {
        IEnumerable<ShiftWorksViewModel> GetAll();

        CreateShiftWorkViewModel CreateModel();

        Task CreateAsync(CreateShiftWorkViewModel model, string user);

        EditShiftWorkViewModel? GetForEdit(int id);

        Task EditAsync(int id, EditShiftWorkViewModel model, string user);

        Task DeleteAsync(int id, string user);

        Task UndoDeleteAsync(int id, string user);

        Task<bool> ExistsAsync(int locomotiveId, DateTime date, Shift shift, int? excludeId = null);
        }
    }
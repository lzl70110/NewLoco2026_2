using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Service.Core.Services
    {
    public class ShiftWorkService : IShiftWorkService
        {
        private readonly LocoDbContext context;

        public ShiftWorkService(LocoDbContext context)
            {
            this.context = context;
            }

        public IEnumerable<ShiftWorksViewModel> GetAll()
            {
            return context.ShiftWorks
                .AsNoTracking()
                .Include(sw => sw.Locomotive)
                .OrderByDescending(sw => sw.Date)
                .ThenBy(sw => sw.Shift)
                .Select(sw => new ShiftWorksViewModel
                    {
                    Id = sw.Id,
                    LocomotiveId = sw.LocoId,
                    Locomotive = sw.Locomotive.Number,
                    Date = sw.Date,
                    Shift = sw.Shift,
                    StartValue=sw.InitialValue,
                    EndValue= sw.FinalValue,
                    
                    CreatedBy = sw.CreatedBy,  
                    CreatedOn = sw.CreatedOn,
                    ModifiedBy = sw.ModifiedBy,
                    ModifiedOn = sw.ModifiedOn,
                    Note = sw.Note,
                    IsDeleted = sw.IsDeleted
                    })
                .ToList();
            }

        public CreateShiftWorkViewModel CreateModel()
            => new CreateShiftWorkViewModel
                {
                Date = DateTime.Today,
                Shift = Shift.Day
                };

        public async Task CreateAsync(CreateShiftWorkViewModel model, string user)
            {
            var exists = await ExistsAsync(model.LocomotiveId, model.Date, model.Shift, null);
            if (exists)
                throw new InvalidOperationException("Duplicate shift for locomotive, date and shift.");

            var amount = model.FinalValue - model.InitialValue;

            var entity = new ShiftWork
                {
                LocoId = model.LocomotiveId,
                Date = model.Date,
                Shift = model.Shift,
                InitialValue = model.InitialValue,
                FinalValue = model.FinalValue,
                Amount = amount,
                Note = model.Note,
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
                };

            await context.ShiftWorks.AddAsync(entity);
            await context.SaveChangesAsync();
            }

        public EditShiftWorkViewModel? GetForEdit(int id)
            {
            return context.ShiftWorks
                .AsNoTracking()
                .Where(sw => sw.Id == id)
                .Select(sw => new EditShiftWorkViewModel
                    {
                    Id = sw.Id,
                    LocomotiveId = sw.LocoId,
                    LocomotiveNumber=sw.Locomotive.Number,
                    Date = sw.Date,
                    Shift = sw.Shift,
                    InitialValue = sw.InitialValue,
                    FinalValue = sw.FinalValue,
                    Amount = sw.Amount,
                    Note = sw.Note,
                   
                    })
                .FirstOrDefault();
            }

        public async Task EditAsync(int id, EditShiftWorkViewModel model, string user)
            {
            var entity = await context.ShiftWorks.FindAsync(id);
            if (entity == null) return;

            var exists = await ExistsAsync(model.LocomotiveId, model.Date, model.Shift, id);
            if (exists)
                throw new InvalidOperationException("Duplicate shift for locomotive, date and shift.");

            entity.LocoId = model.LocomotiveId;
            entity.Date = model.Date;
            entity.Shift = model.Shift;
            entity.InitialValue = model.InitialValue;
            entity.FinalValue = model.FinalValue;
            entity.Amount = model.FinalValue - model.InitialValue;
            entity.Note = model.Note;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            }

        public async Task DeleteAsync(int id, string user)
            {
            var entity = await context.ShiftWorks.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            }

        public async Task UndoDeleteAsync(int id, string user)
            {
            var entity = await context.ShiftWorks.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = false;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            }

        public async Task<bool> ExistsAsync(int locomotiveId, DateTime date, Shift shift, int? excludeId = null)
            {
            var query = context.ShiftWorks
                .AsNoTracking()
                .Where(sw => sw.LocoId == locomotiveId
                             && sw.Date == date
                             && sw.Shift == shift
                             && !sw.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(sw => sw.Id != excludeId.Value);

            return await query.AnyAsync();
            }
        }
    }
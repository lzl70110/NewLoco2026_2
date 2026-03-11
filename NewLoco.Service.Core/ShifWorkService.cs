using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.ShiftWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewLoco.Service.Core.Services
    {
    public class ShiftWorkService : IShiftWorkService
        {
        private readonly LocoDbContext context;
        private readonly IFuelService fuelService;
        private const decimal MinFuel = 100m;

        public ShiftWorkService(LocoDbContext context, IFuelService fuelService)
            {
            this.context = context;
            this.fuelService = fuelService;
            }

        // Get all shift records
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
                    StartValue = sw.InitialValue,
                    EndValue = sw.FinalValue,
                    CreatedBy = sw.CreatedBy,
                    CreatedOn = sw.CreatedOn,
                    ModifiedBy = sw.ModifiedBy,
                    ModifiedOn = sw.ModifiedOn,
                    Note = sw.Note,
                    IsDeleted = sw.IsDeleted
                    })
                .ToList();
            }

        // Create default model for new shift
        public CreateShiftWorkViewModel CreateModel()
            => new CreateShiftWorkViewModel
                {
                Date = DateTime.Today,
                Shift = Shift.Day
                };

        // Get last shift as DTO
        public async Task<ShiftWorkDto?> GetLastShiftAsync(int locomotiveId)
            {
            var last = await context.ShiftWorks
                .Where(sw => sw.LocoId == locomotiveId && !sw.IsDeleted)
                .OrderByDescending(sw => sw.Date)
                .ThenByDescending(sw => sw.Id)
                .FirstOrDefaultAsync();

            if (last == null) return null;

            return new ShiftWorkDto
                {
                Id = last.Id,
                LocoId = last.LocoId,
                Date = last.Date,
                FinalValue = last.FinalValue
                };
            }

        // Create new shift
        public async Task CreateAsync(CreateShiftWorkViewModel model, string user)
            {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException("Shift date cannot be in the future.");

            if (await ExistsAsync(model.LocomotiveId, model.Date, model.Shift, null))
                throw new InvalidOperationException("Duplicate shift for locomotive, date and shift.");

            decimal lastFuel = await fuelService.GetPrevFinalAsync(model.LocomotiveId, model.Date);

            if (lastFuel < MinFuel)
                throw new InvalidOperationException($"Cannot start shift: fuel below minimum ({MinFuel} liters).");

            decimal consumption = model.FinalValue - model.InitialValue;
            if (consumption <= 0)
                throw new InvalidOperationException("Shift work must consume fuel.");

            if (consumption > lastFuel)
                throw new InvalidOperationException("Not enough fuel for this shift.");

            var entity = new ShiftWork
                {
                LocoId = model.LocomotiveId,
                Date = model.Date,
                Shift = model.Shift,
                InitialValue = model.InitialValue,
                FinalValue = model.FinalValue,
                Amount = consumption,
                Note = model.Note,
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
                };

            await context.ShiftWorks.AddAsync(entity);
            await fuelService.ConsumeFuelAsync(model.LocomotiveId, consumption, user);
            await context.SaveChangesAsync();
            }

        // Get shift for editing
        public EditShiftWorkViewModel? GetForEdit(int id)
            {
            return context.ShiftWorks
                .AsNoTracking()
                .Where(sw => sw.Id == id)
                .Select(sw => new EditShiftWorkViewModel
                    {
                    Id = sw.Id,
                    LocomotiveId = sw.LocoId,
                    Date = sw.Date,
                    Shift = sw.Shift,
                    StartValue = sw.InitialValue,
                    EndValue = sw.FinalValue,
                    Note = sw.Note
                    })
                .FirstOrDefault();
            }

        // Edit shift
        public async Task EditAsync(int id, EditShiftWorkViewModel model, string user)
            {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException("Shift date cannot be in the future.");

            var entity = await context.ShiftWorks.FindAsync(id);
            if (entity == null) return;

            if (await ExistsAsync(model.LocomotiveId, model.Date, model.Shift, id))
                throw new InvalidOperationException("Duplicate shift for locomotive, date and shift.");

            decimal newConsumption = model.EndValue - model.StartValue;
            if (newConsumption <= 0)
                throw new InvalidOperationException("Shift work must consume fuel.");

            decimal lastFuel = await fuelService.GetPrevFinalAsync(model.LocomotiveId, model.Date);
            if (lastFuel < MinFuel)
                throw new InvalidOperationException($"Cannot start shift: fuel below minimum ({MinFuel} liters).");

            decimal oldConsumption = entity.Amount;
            if (entity.LocoId != model.LocomotiveId || newConsumption != oldConsumption)
                {
                await fuelService.ConsumeFuelAsync(entity.LocoId, -oldConsumption, user);
                await fuelService.ConsumeFuelAsync(model.LocomotiveId, newConsumption, user);
                }

            entity.LocoId = model.LocomotiveId;
            entity.Date = model.Date;
            entity.Shift = model.Shift;
            entity.InitialValue = model.StartValue;
            entity.FinalValue = model.EndValue;
            entity.Amount = newConsumption;
            entity.Note = model.Note;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            }

        // Soft delete shift
        public async Task DeleteAsync(int id, string user)
            {
            var entity = await context.ShiftWorks.FindAsync(id);
            if (entity == null) return;

            await fuelService.ConsumeFuelAsync(entity.LocoId, -entity.Amount, user);

            entity.IsDeleted = true;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            }

        // Undo deletion
        public async Task UndoDeleteAsync(int id, string user)
            {
            var entity = await context.ShiftWorks.FindAsync(id);
            if (entity == null) return;

            await fuelService.ConsumeFuelAsync(entity.LocoId, entity.Amount, user);

            entity.IsDeleted = false;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();
            }

        // Check for duplicate shift
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

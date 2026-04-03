using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GCommon; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewLoco.Data;
using NewLoco.Data.Models.Fuel;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Service.Core.Services
{
    public class FuelService : IFuelService
    {
        private readonly LocoDbContext _context;
        private readonly FuelPoliciesOptions _policies;
        private readonly int _depotStep;

        public FuelService(LocoDbContext db, IOptions<FuelPoliciesOptions> opts)
        {
            _context = db ?? throw new ArgumentNullException(nameof(db));

            _policies = (opts ?? throw new ArgumentNullException(nameof(opts))).Value
                ?? throw new ArgumentException("Fuel policies not configured.");

            _depotStep = Math.Max(1, _policies.DepotStepLiters);
        }

        // -------------------------- Helpers --------------------------

        private bool IsMultipleOfStep(decimal v) =>
            v >= 0 && v % _depotStep == 0;

        private void EnsureMultipleOfStep(decimal v)
        {
            if (!IsMultipleOfStep(v))
                throw new InvalidOperationException("Fuel amount must be aligned to depot step.");
        }

        private static void EnsureNonNegative(params (decimal value, string name)[] pairs)
        {
            foreach (var (value, _) in pairs)
                if (value < 0)
                    throw new InvalidOperationException("Negative fuel amount is not allowed.");
        }

        private FuelSafetyOptions GetSafetyFor(string? locoNumber)
        {
            var cls = (locoNumber ?? "").Split('-', StringSplitOptions.TrimEntries).FirstOrDefault() ?? "";

            return _policies.PerClassSafety != null &&
                   _policies.PerClassSafety.TryGetValue(cls, out var s)
                ? s
                : new FuelSafetyOptions { HardFloorLiters = 100, SoftWarningLiters = 120 };
        }

        private static Shift ExtractShift(object vm, Shift fallback)
        {
            var prop = vm.GetType().GetProperty("Shift");
            if (prop == null) return fallback;

            var v = prop.GetValue(vm);
            return v is Shift s ? s : fallback;
        }

        // -------------------------- Legacy UI Queries --------------------------

        public IEnumerable<FuelAllViewModel> GetAll()
        {
            return _context.Fuels
                .AsNoTracking()
                .Include(f => f.Locomotive)
                .Select(f => new FuelAllViewModel
                {
                    Id = f.Id,
                    LocomotiveId = f.LocoId,
                    LocomotiveNumber = f.Locomotive.Number,
                    Date = f.Date,
                    InitialFuel = f.InitialFuel,
                    FinalFuel = f.FinalFuel,
                    Consumption = f.Consumption,
                    Refueled = f.Refueled,
                    Note = f.Note ?? "",
                    IsDeleted = f.IsDeleted,
                    CreatedOn = f.CreatedOn,
                    CreatedByUserName = f.CreatedBy,
                    EditedBy = f.ModifiedBy,
                    EditedOn = f.ModifiedOn
                })
                .ToList();
        }

        public IEnumerable<FuelsBasicDetailsViewModel> GetForIndexLatest()
        {
            var latestIds = _context.Fuels
                .Where(f => !f.IsDeleted)
                .GroupBy(f => f.LocoId)
                .Select(g =>
                    g.OrderByDescending(x => x.Date)
                     .ThenByDescending(x => x.Id)
                     .First().Id)
                .ToList();

            return _context.Fuels
                .AsNoTracking()
                .Include(f => f.Locomotive)
                .Where(f => latestIds.Contains(f.Id))
                .Select(f => new FuelsBasicDetailsViewModel
                {
                    Id = f.Id,
                    LocomotiveId = f.LocoId,
                    LocomotiveNumber = f.Locomotive.Number,
                    Date = f.Date,
                    InitialFuel = f.InitialFuel,
                    FinalFuel = f.FinalFuel,
                    IsDeleted = f.IsDeleted
                })
                .OrderBy(x => x.LocomotiveNumber)
                .ToList();
        }

        public CreateFuelViewModel CreateModel() =>
            new CreateFuelViewModel
            {
                Date = DateTime.Today,
                InitialFuel = 0
            };

        // -------------------------- Legacy Create --------------------------

        public async Task CreateAsync(CreateFuelViewModel model, string user)
        {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException("Fuel entry cannot be in the future.");

            EnsureNonNegative((model.FinalFuel, "FinalFuel"), (model.Refueled, "Refueled"));
            EnsureMultipleOfStep(model.Refueled);

            var shift = ExtractShift(model, Shift.Day);
            var initial = await GetPrevFinalAsync(model.LocomotiveId, model.Date, shift);

            if (initial == 0 && model.Refueled == 0)
                throw new InvalidOperationException("Cannot create first row without refuel.");

            if (model.FinalFuel > initial + model.Refueled)
                throw new InvalidOperationException("Final fuel cannot exceed initial + refueled.");

            var consumption = (initial + model.Refueled) - model.FinalFuel;
            EnsureMultipleOfStep(consumption);

            var locoNumber = await _context.Locomotives
                .Where(l => l.Id == model.LocomotiveId)
                .Select(l => l.Number)
                .FirstOrDefaultAsync();

            var safety = GetSafetyFor(locoNumber);
            if (model.FinalFuel < safety.HardFloorLiters)
                throw new InvalidOperationException($"Final fuel cannot be below hard floor {safety.HardFloorLiters}L.");

            var entity = new Fuel
            {
                LocoId = model.LocomotiveId,
                Date = model.Date,
                Shift = shift,
                InitialFuel = initial,
                FinalFuel = model.FinalFuel,
                Refueled = model.Refueled,
                Consumption = consumption,
                Note = model.Note ?? "",
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow
            };

            _context.Fuels.Add(entity);
            await _context.SaveChangesAsync();
        }

        // -------------------------- Legacy GetForEdit --------------------------

        public FuelAllViewModel? GetForEdit(int id)
        {
            return _context.Fuels
                .AsNoTracking()
                .Include(f => f.Locomotive)
                .Where(f => f.Id == id)
                .Select(f => new FuelAllViewModel
                {
                    Id = f.Id,
                    LocomotiveId = f.LocoId,
                    LocomotiveNumber = f.Locomotive.Number,
                    Date = f.Date,
                    InitialFuel = f.InitialFuel,
                    FinalFuel = f.FinalFuel,
                    Consumption = f.Consumption,
                    Refueled = f.Refueled,
                    Note = f.Note ?? "",
                    IsDeleted = f.IsDeleted,
                    CreatedOn = f.CreatedOn,
                    CreatedByUserName = f.CreatedBy,
                    EditedBy = f.ModifiedBy,
                    EditedOn = f.ModifiedOn
                })
                .FirstOrDefault();
        }

        // -------------------------- Legacy Edit --------------------------

        public async Task EditAsync(int id, FuelAllViewModel model, string user)
        {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException("Fuel entry cannot be in the future.");

            EnsureNonNegative((model.FinalFuel, "FinalFuel"), (model.Refueled, "Refueled"));
            EnsureMultipleOfStep(model.Refueled);

            var entity = await _context.Fuels.FindAsync(id);
            if (entity == null) return;

            var shift = ExtractShift(model, entity.Shift == 0 ? Shift.Day : entity.Shift);

            var initial = entity.InitialFuel; // <- FIXED: never recalc

            if (initial == 0 && model.Refueled == 0)
                throw new InvalidOperationException("Cannot edit first row without refuel.");

            if (model.FinalFuel > initial + model.Refueled)
                throw new InvalidOperationException("Final fuel cannot exceed initial + refueled.");

            var consumption = (initial + model.Refueled) - model.FinalFuel;
            EnsureMultipleOfStep(consumption);

            var locoNumber = await _context.Locomotives
                .Where(l => l.Id == entity.LocoId)
                .Select(l => l.Number)
                .FirstOrDefaultAsync();

            var safety = GetSafetyFor(locoNumber);
            if (model.FinalFuel < safety.HardFloorLiters)
                throw new InvalidOperationException($"Final fuel cannot be below hard floor {safety.HardFloorLiters}L.");

            entity.Date = model.Date;
            entity.Shift = shift;
            entity.InitialFuel = initial;
            entity.FinalFuel = model.FinalFuel;
            entity.Refueled = model.Refueled;
            entity.Consumption = consumption;
            entity.Note = model.Note ?? "";
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // -------------------------- Delete / Undo --------------------------

        public async Task DeleteAsync(int id, string user)
        {
            var entity = await _context.Fuels.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UndoDeleteAsync(int id, string user)
        {
            var entity = await _context.Fuels.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = false;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // -------------------------- Lookups --------------------------

        public decimal GetLastFuel(int locomotiveId)
        {
            return _context.Fuels
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefault();
        }

        public async Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date)
        {
            return await _context.Fuels
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted && f.Date < date)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date, Shift shift, CancellationToken ct = default)
        {
            return await _context.Fuels
                .Where(f =>
                    f.LocoId == locomotiveId &&
                    !f.IsDeleted &&
                    (f.Date < date || (f.Date == date && f.Shift < shift)))
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Shift).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync(ct);
        }

        // -------------------------- Stock / Refuel / Consume --------------------------

        public async Task<decimal> GetCurrentStockAsync(int locomotiveId, CancellationToken ct = default)
        {
            return await _context.Fuels
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync(ct);
        }

        public async Task RefuelAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default)
        {
            if (liters <= 0)
                throw new InvalidOperationException("Fuel amount must be positive.");

            EnsureMultipleOfStep(liters);

            var today = DateTime.Today;

            var row = await _context.Fuels
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted && f.Date == today)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct);

            if (row != null)
            {
                row.Refueled += liters;
                row.FinalFuel += liters;
                row.ModifiedBy = user;
                row.ModifiedOn = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(note))
                {
                    row.Note = string.IsNullOrEmpty(row.Note)
                        ? note
                        : $"{row.Note}; {note}";
                }

                await _context.SaveChangesAsync(ct);
                return;
            }

            var initial = await GetPrevFinalAsync(locomotiveId, today);

            var entity = new Fuel
            {
                LocoId = locomotiveId,
                Date = today,
                Shift = Shift.Day,
                InitialFuel = initial,
                Refueled = liters,
                FinalFuel = initial + liters,
                Consumption = 0,
                Note = note ?? "",
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow
            };

            _context.Fuels.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task ConsumeAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default)
        {
            await ConsumeOnAsync(locomotiveId, DateTime.Today, Shift.Day, liters, user, note, ct);
        }

        public async Task ConsumeOnAsync(int locomotiveId, DateTime date, Shift shift, int liters, string user, string? note = null, CancellationToken ct = default)
        {
            if (liters <= 0) return;

            EnsureMultipleOfStep(liters);

            var last = await _context.Fuels
                .Include(f => f.Locomotive)
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("No previous record for this locomotive.");

            var safety = GetSafetyFor(last.Locomotive?.Number);

            var row = await _context.Fuels
                .Where(f => f.LocoId == locomotiveId &&
                            !f.IsDeleted &&
                            f.Date == date &&
                            f.Shift == shift)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct);

            if (row != null)
            {
                var projected = row.FinalFuel - liters;

                if (projected < safety.HardFloorLiters)
                    throw new InvalidOperationException($"Final fuel cannot be below hard floor {safety.HardFloorLiters}L.");

                row.FinalFuel = projected;
                row.Consumption += liters;
                row.ModifiedBy = user;
                row.ModifiedOn = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(note))
                {
                    row.Note = string.IsNullOrEmpty(row.Note)
                        ? note
                        : $"{row.Note}; {note}";
                }

                await _context.SaveChangesAsync(ct);
                return;
            }

            var initial = await GetPrevFinalAsync(locomotiveId, date, shift, ct);

            if (initial <= 0)
                throw new InvalidOperationException("No previous fuel entry available.");

            var final = initial - liters;

            if (final < safety.HardFloorLiters)
                throw new InvalidOperationException($"Final fuel cannot be below hard floor {safety.HardFloorLiters}L.");

            var entity = new Fuel
            {
                LocoId = locomotiveId,
                Date = date,
                Shift = shift,
                InitialFuel = initial,
                Consumption = liters,
                FinalFuel = final,
                Note = note ?? "",
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow
            };

            _context.Fuels.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task ConsumeFuelAsync(int locomotiveId, decimal amount, string user)
        {
            if (amount <= 0) return;

            EnsureNonNegative((amount, nameof(amount)));
            EnsureMultipleOfStep(amount);

            var liters = (int)Math.Round(amount, 0, MidpointRounding.AwayFromZero);

            await ConsumeAsync(locomotiveId, liters, user);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GCommon; // Messages
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewLoco.Data;
using NewLoco.Data.Models.Fuel;
using NewLoco.GCommon.Enums; // Shift
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
                        ?? throw new ArgumentException(Messages.Fuel.Error_PoliciesNotConfigured);

            _depotStep = Math.Max(1, _policies.DepotStepLiters);
        }

        // ------------------------- helpers -------------------------

        private bool IsMultipleOfStep(decimal value) => value >= 0 && value % _depotStep == 0;

        private void EnsureMultipleOfStep(decimal value)
        {
            if (!IsMultipleOfStep(value))
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_FuelAmountMustBeMultipleOf10);
        }

        private static void EnsureNonNegative(params (decimal v, string name)[] pairs)
        {
            foreach (var (v, _) in pairs)
                if (v < 0) throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_InvalidFuelAmount);
        }

        private FuelSafetyOptions GetSafetyFor(string? locoNumber)
        {
            var cls = (locoNumber ?? string.Empty).Split('-', StringSplitOptions.TrimEntries)
                                                  .FirstOrDefault() ?? string.Empty;
            return _policies.PerClassSafety != null
                && _policies.PerClassSafety.TryGetValue(cls, out var s)
                ? s
                : new FuelSafetyOptions { SoftWarningLiters = 120, HardFloorLiters = 100 }; // fallback
        }

        private static Shift TryGetShiftFrom(object vm, Shift defaultValue)
        {
            var prop = vm.GetType().GetProperty("Shift");
            if (prop == null) return defaultValue;
            var value = prop.GetValue(vm);
            return value is Shift s ? s : defaultValue;
        }

        // ------------------------- queries for UI -------------------------

        public IEnumerable<FuelAllViewModel> GetAll()
        {
            return [.. _context.Fuels
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
                    Note = f.Note ?? string.Empty,
                    IsDeleted = f.IsDeleted,
                    CreatedOn = f.CreatedOn,
                    CreatedByUserName = f.CreatedBy,
                    EditedBy = f.ModifiedBy,
                    EditedOn = f.ModifiedOn
                })];
        }

        public IEnumerable<FuelsBasicDetailsViewModel> GetForIndexLatest()
        {
            var latestIds = _context.Fuels
                .AsNoTracking()
                .Where(f => !f.IsDeleted)
                .GroupBy(f => f.LocoId)
                .Select(g => g.OrderByDescending(x => x.Date)
                              .ThenByDescending(x => x.Id)
                              .Select(x => x.Id)
                              .FirstOrDefault())
                .ToList();

            if (latestIds.Count == 0) return [];

            var result = _context.Fuels
                .AsNoTracking()
                .Where(f => latestIds.Contains(f.Id))
                .Include(f => f.Locomotive)
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

            return result;
        }

        public CreateFuelViewModel CreateModel()
            => new()
            {
                Date = DateTime.Today,
                InitialFuel = 0
            };

        // ------------------------- create / edit daily row -------------------------

        public async Task CreateAsync(CreateFuelViewModel model, string user)
        {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_FuelInFuture);

            EnsureNonNegative((model.FinalFuel, nameof(model.FinalFuel)),
                              (model.Refueled, nameof(model.Refueled)));           
            EnsureMultipleOfStep(model.Refueled);

            var shift = TryGetShiftFrom(model, Shift.Day);
            var initial = await GetPrevFinalAsync(model.LocomotiveId, model.Date, shift);

            if (initial == 0 && model.Refueled == 0)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_NoFuelRecordForLoco);

            if (model.FinalFuel > initial + model.Refueled)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_FinalFuelTooHigh);

            var consumption = (initial + model.Refueled) - model.FinalFuel;
            EnsureMultipleOfStep(consumption);

            // per-class hard floor check
            var locoNumber = await _context.Locomotives
                .Where(l => l.Id == model.LocomotiveId)
                .Select(l => l.Number)
                .FirstOrDefaultAsync();

            var safety = GetSafetyFor(locoNumber);
            if (model.FinalFuel < safety.HardFloorLiters)
                throw new InvalidOperationException(
                    string.Format(Messages.FuelServiceKeys.Msg_FinalBelowHardFloorFmt, safety.HardFloorLiters));

            var entity = new NewLoco.Data.Models.Fuel.Fuel
            {
                LocoId = model.LocomotiveId,
                Date = model.Date,
                Shift = shift,
                InitialFuel = initial,
                FinalFuel = model.FinalFuel,
                Refueled = model.Refueled,
                Consumption = consumption,
                Note = model.Note ?? string.Empty,
                IsDeleted = false,
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow
            };

            _context.Fuels.Add(entity);
            await _context.SaveChangesAsync();
        }

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
                    Note = f.Note ?? string.Empty,
                    IsDeleted = f.IsDeleted,
                    CreatedOn = f.CreatedOn,
                    CreatedByUserName = f.CreatedBy,
                    EditedBy = f.ModifiedBy,
                    EditedOn = f.ModifiedOn
                })
                .FirstOrDefault();
        }

        public async Task EditAsync(int id, FuelAllViewModel model, string user)
        {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_FuelInFuture);

            EnsureNonNegative((model.FinalFuel, nameof(model.FinalFuel)),
                              (model.Refueled, nameof(model.Refueled)));
            EnsureMultipleOfStep(model.Refueled);

            var entity = await _context.Fuels.FindAsync(id);
            if (entity == null) return;

            var shift = TryGetShiftFrom(model, entity.Shift == 0 ? Shift.Day : entity.Shift);
            var initial = await GetPrevFinalAsync(entity.LocoId, model.Date, shift);

            if (initial == 0 && model.Refueled == 0)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_NoFuelRecordForLoco);

            if (model.FinalFuel > initial + model.Refueled)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_FinalFuelTooHigh);

            var consumption = (initial + model.Refueled) - model.FinalFuel;
            EnsureMultipleOfStep(consumption);

            var locoNumber = await _context.Locomotives
                .Where(l => l.Id == entity.LocoId)
                .Select(l => l.Number)
                .FirstOrDefaultAsync();

            var safety = GetSafetyFor(locoNumber);
            if (model.FinalFuel < safety.HardFloorLiters)
                throw new InvalidOperationException(
                    string.Format(Messages.FuelServiceKeys.Msg_FinalBelowHardFloorFmt, safety.HardFloorLiters));

            entity.Date = model.Date;
            entity.Shift = shift;
            entity.InitialFuel = initial;
            entity.FinalFuel = model.FinalFuel;
            entity.Refueled = model.Refueled;
            entity.Consumption = consumption;
            entity.Note = model.Note ?? string.Empty;
            entity.ModifiedBy = user;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ------------------------- soft delete / restore -------------------------

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

        // ------------------------- helpers / lookups -------------------------

        public decimal GetLastFuel(int locomotiveId)
        {
            return _context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefault();
        }

        public async Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date)
        {
            return await _context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted && f.Date < date)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync();
        }

        // --- overload: considers date + shift (same-day second record) ---
        public async Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date, Shift shift, CancellationToken ct = default)
        {
            return await _context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted
                         && (f.Date < date || (f.Date == date && f.Shift < shift)))
                .OrderByDescending(f => f.Date)
                .ThenByDescending(f => f.Shift)
                .ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync(ct);
        }

        // ------------------------- stock / refuel / consume -------------------------

        public async Task<decimal> GetCurrentStockAsync(int locomotiveId, CancellationToken ct = default)
        {
            return await _context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync(ct);
        }

        public async Task RefuelAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default)
        {
            if (liters <= 0) throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_InvalidFuelAmount);
            EnsureMultipleOfStep(liters);

            var today = DateTime.Today;

            // If there's already a row for today -> update refueled/final on it, else create
            var todayRow = await _context.Fuels
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted && f.Date == today)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct);

            if (todayRow != null)
            {
                todayRow.Refueled += liters;
                todayRow.FinalFuel += liters;
                todayRow.ModifiedBy = user;
                todayRow.ModifiedOn = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(note))
                {
                    todayRow.Note = string.IsNullOrWhiteSpace(todayRow.Note)
                        ? note
                        : $"{todayRow.Note}{Messages.Fuel.NotesJoinSeparator}{note}";
                }

                await _context.SaveChangesAsync(ct);
                return;
            }

            var initial = await GetPrevFinalAsync(locomotiveId, today);
            if (initial < 0) initial = 0;

            var entity = new NewLoco.Data.Models.Fuel.Fuel
            {
                LocoId = locomotiveId,
                Date = today,
                Shift = Shift.Day,
                InitialFuel = initial,
                Refueled = liters,
                FinalFuel = initial + liters,
                Consumption = 0,
                Note = note ?? string.Empty,
                IsDeleted = false,
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow
            };

            _context.Fuels.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task ConsumeAsync(int locomotiveId, int liters, string user, string? note = null, CancellationToken ct = default)
        {
            // Default “today/Shift.Day” consumer – if no row for today, create one.
            await ConsumeOnAsync(locomotiveId, DateTime.Today, Shift.Day, liters, user, note, ct);
        }

        public async Task ConsumeOnAsync(int locomotiveId, DateTime date, Shift shift, int liters, string user, string? note = null, CancellationToken ct = default)
        {
            if (liters <= 0) return;
            EnsureMultipleOfStep(liters);

            // last known row (for initial fallback & safety lookup)
            var last = await _context.Fuels
                .Include(f => f.Locomotive)
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_NoFuelRecordForLoco);

            // resolve class thresholds
            var locoNumber = last.Locomotive?.Number;
            if (string.IsNullOrWhiteSpace(locoNumber))
            {
                locoNumber = await _context.Locomotives
                    .Where(l => l.Id == locomotiveId)
                    .Select(l => l.Number)
                    .FirstOrDefaultAsync(ct);
            }
            var safety = GetSafetyFor(locoNumber);

            // If row for (date, shift) exists -> update it
            var row = await _context.Fuels
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted && f.Date == date && f.Shift == shift)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct);

            if (row != null)
            {
                var projectedFinal = row.FinalFuel - liters;
                if (projectedFinal < safety.HardFloorLiters)
                    throw new InvalidOperationException(
                        string.Format(Messages.FuelServiceKeys.Msg_FinalBelowHardFloorFmt, safety.HardFloorLiters));

                row.FinalFuel = projectedFinal;
                row.Consumption += liters;

                if (!string.IsNullOrWhiteSpace(note))
                {
                    row.Note = string.IsNullOrWhiteSpace(row.Note)
                        ? note
                        : $"{row.Note}{Messages.Fuel.NotesJoinSeparator}{note}";
                }

                row.ModifiedBy = user;
                row.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);
                return;
            }

            // Else: create a new daily row for (date, shift)
            var initial = await GetPrevFinalAsync(locomotiveId, date, shift, ct);
            if (initial <= 0)
                throw new InvalidOperationException(Messages.FuelServiceKeys.Msg_NoFuelRecordForLoco);

            var final = initial - liters;
            if (final < safety.HardFloorLiters)
                throw new InvalidOperationException(
                    string.Format(Messages.FuelServiceKeys.Msg_FinalBelowHardFloorFmt, safety.HardFloorLiters));

            var entity = new NewLoco.Data.Models.Fuel.Fuel
            {
                LocoId = locomotiveId,
                Date = date,
                Shift = shift,
                InitialFuel = initial,
                Refueled = 0,
                Consumption = liters,
                FinalFuel = final,
                Note = note ?? string.Empty,
                IsDeleted = false,
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow
            };

            _context.Fuels.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        /// legacy forwarder (decimal -> int, same depot rule)
        public async Task ConsumeFuelAsync(int locomotiveId, decimal amount, string user)
        {
            if (amount <= 0) return;
            EnsureNonNegative((amount, nameof(amount)));
            EnsureMultipleOfStep(amount);

            var liters = (int)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
            await ConsumeAsync(locomotiveId, liters, user, note: null);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core.Contracts;          // IShiftWorkService, ShiftWorkDto, ShiftWorkQuery
using NewLoco.Web.ViewModels.ShiftWorks;       // CreateShiftWorkViewModel, EditShiftWorkViewModel

namespace NewLoco.Service.Core
{
    public class ShiftWorkService(LocoDbContext db, ILogger<ShiftWorkService> logger) : IShiftWorkService
    {
        private readonly LocoDbContext _db = db ?? throw new ArgumentNullException(nameof(db));                // change: DI for DbContext (fix "_db does not exist")
        private readonly ILogger<ShiftWorkService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Factory за Create VM
        public CreateShiftWorkViewModel CreateModel()
        {
            return new CreateShiftWorkViewModel
            {
                Date = DateTime.Today,
                Shift = NewLoco.GCommon.Enums.Shift.Day,
                InitialValue = 0,
                FinalValue = 0,
                Note = null
            };
        }

        // Търсене + странициране
        public async Task<(IEnumerable<ShiftWorkDto> Items, int Total)> GetAllAsync(ShiftWorkQuery query, CancellationToken ct = default)
        {
            var q = _db.ShiftWorks
                .AsNoTracking()
                .Include(sw => sw.Locomotive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.LocomotiveNumber))
            {
                var number = query.LocomotiveNumber.Trim();
                q = q.Where(sw => sw.Locomotive != null && sw.Locomotive.Number.Contains(number));
            }



            if (query.From.HasValue)
                q = q.Where(sw => sw.Date >= query.From.Value.Date);

            if (query.To.HasValue)
            {
                var next = query.To.Value.Date.AddDays(1);
                q = q.Where(sw => sw.Date < next);
            }



            if (!query.IncludeDeleted) q = q.Where(sw => !sw.IsDeleted);

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(sw => sw.Date).ThenByDescending(sw => sw.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(sw => new ShiftWorkDto
                {
                    Id = sw.Id,
                    LocoId = sw.LocomotiveId,
                    LocomotiveNumber = sw.Locomotive != null ? sw.Locomotive.Number : string.Empty,
                    Date = sw.Date,                         // DTO e DateTime – директно
                    Shift = sw.Shift,
                    Operator = sw.CreatedBy ?? string.Empty,
                    InitialValue = sw.InitialValue,
                    FinalValue = sw.FinalValue,
                    Amount = sw.Amount,
                    Note = sw.Note,
                    IsDeleted = sw.IsDeleted
                })
                .ToListAsync(ct);

            return (items, total);
        }

        // Последна смяна за локомотив (по дата/id)
        public async Task<ShiftWorkDto?> GetLastShiftAsync(int locomotiveId)
        {
            var last = await _db.ShiftWorks
                .AsNoTracking()
                .Include(sw => sw.Locomotive)
                .Where(sw => sw.LocomotiveId == locomotiveId && !sw.IsDeleted)
                .OrderByDescending(sw => sw.Date).ThenByDescending(sw => sw.Id)
                .Select(sw => new ShiftWorkDto
                {
                    Id = sw.Id,
                    LocoId = sw.LocomotiveId,
                    LocomotiveNumber = sw.Locomotive != null ? sw.Locomotive.Number : string.Empty,
                    Date = sw.Date,
                    Shift = sw.Shift,
                    Operator = sw.CreatedBy ?? string.Empty,
                    InitialValue = sw.InitialValue,
                    FinalValue = sw.FinalValue,
                    Amount = sw.Amount,
                    Note = sw.Note,
                    IsDeleted = sw.IsDeleted
                })
                .FirstOrDefaultAsync();

            return last;
        }

        // Зареждане за редакция
        public EditShiftWorkViewModel? GetForEdit(int id)
        {
            var e = _db.ShiftWorks.Find(id); // sync read
            if (e == null) return null;

            return new EditShiftWorkViewModel
            {
                Id = e.Id,
                LocomotiveId = e.LocomotiveId,
                Date = e.Date,            // DateTime
                Shift = e.Shift,
                InitialValue = e.InitialValue,
                FinalValue = e.FinalValue,
                Note = e.Note
            };
        }

        // Create
        public async Task CreateAsync(CreateShiftWorkViewModel model, string user)
        {
            if (model.FinalValue <= model.InitialValue)
                throw new InvalidOperationException("Final counter must be greater than the initial counter.");

            var entity = new ShiftWork
            {
                LocomotiveId = model.LocomotiveId,
                Date = model.Date.Date,   // change: нулираме часове за всеки случай
                Shift = model.Shift,
                InitialValue = model.InitialValue,
                FinalValue = model.FinalValue,
                Amount = model.FinalValue - model.InitialValue,
                Note = model.Note,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user
            };

            _db.ShiftWorks.Add(entity);
            await _db.SaveChangesAsync();
        }

        // Edit
        public async Task EditAsync(int id, EditShiftWorkViewModel model, string user)
        {
            var e = await _db.ShiftWorks.FirstOrDefaultAsync(x => x.Id == id) ?? throw new InvalidOperationException("ShiftWork not found.");
            e.LocomotiveId = model.LocomotiveId;
            e.Date = model.Date.Date;  // change: truncate time
            e.Shift = model.Shift;
            e.InitialValue = model.InitialValue;
            e.FinalValue = model.FinalValue;
            e.Amount = model.FinalValue - model.InitialValue;
            e.Note = model.Note;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // Soft delete
        public async Task DeleteAsync(int id, string user)
        {
            var e = await _db.ShiftWorks.FirstOrDefaultAsync(x => x.Id == id);
            if (e == null || e.IsDeleted) return;

            e.IsDeleted = true;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // Undo soft delete
        public async Task UndoDeleteAsync(int id, string user)
        {
            var e = await _db.ShiftWorks.FirstOrDefaultAsync(x => x.Id == id);
            if (e == null || !e.IsDeleted) return;

            e.IsDeleted = false;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Service.Core
{
    public class ShiftWorkService : IShiftWorkService
    {
        private readonly LocoDbContext _db;
        private readonly ILogger<ShiftWorkService> _logger;

        public ShiftWorkService(LocoDbContext db, ILogger<ShiftWorkService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ----------------------------------------------------------
        // FACTORY MODEL
        // ----------------------------------------------------------
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

        // ----------------------------------------------------------
        // GET ALL (FILTERED + PAGED)
        // ----------------------------------------------------------
        public async Task<(IEnumerable<ShiftWorkDto> Items, int Total)> GetAllAsync(
            ShiftWorkQuery query,
            CancellationToken ct = default)
        {
            // Safety
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);

            IQueryable<ShiftWork> q;

            if (query.IncludeDeleted)
            {
                q = _db.ShiftWorks
                    .IgnoreQueryFilters(); // 🔥 CRITICAL FIX
            }
            else
            {
                q = _db.ShiftWorks
                    .Where(sw => !sw.IsDeleted);
            }

            q = q
                .AsNoTracking()
                .Include(sw => sw.Locomotive);

            // ------------------------------------------------------
            // FILTERS
            // ------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(query.LocomotiveNumber))
            {
                var number = query.LocomotiveNumber.Trim();

                q = q.Where(sw =>
                    sw.Locomotive != null &&
                    sw.Locomotive.Number.Contains(number));
            }

            if (query.From.HasValue)
                q = q.Where(sw => sw.Date >= query.From.Value.Date);

            if (query.To.HasValue)
            {
                var next = query.To.Value.Date.AddDays(1);
                q = q.Where(sw => sw.Date < next);
            }

            // ------------------------------------------------------
            // COUNT
            // ------------------------------------------------------
            var total = await q.CountAsync(ct);

            // ------------------------------------------------------
            // DATA
            // ------------------------------------------------------
            var items = await q
                .OrderByDescending(sw => sw.Date)
                .ThenByDescending(sw => sw.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .ToListAsync(ct);

            return (items, total);
        }

        // ----------------------------------------------------------
        // GET LAST SHIFT
        // ----------------------------------------------------------
        public async Task<ShiftWorkDto?> GetLastShiftAsync(int locomotiveId)
        {
            return await _db.ShiftWorks
                .AsNoTracking()
                .Include(sw => sw.Locomotive)
                .Where(sw => sw.LocomotiveId == locomotiveId && !sw.IsDeleted)
                .OrderByDescending(sw => sw.Date)
                .ThenByDescending(sw => sw.Id)
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
        }

        // ----------------------------------------------------------
        // GET FOR EDIT
        // ----------------------------------------------------------
        public EditShiftWorkViewModel? GetForEdit(int id)
        {
            var e = _db.ShiftWorks.Find(id);
            if (e == null) return null;

            return new EditShiftWorkViewModel
            {
                Id = e.Id,
                LocomotiveId = e.LocomotiveId,
                Date = e.Date,
                Shift = e.Shift,
                InitialValue = e.InitialValue,
                FinalValue = e.FinalValue,
                Note = e.Note
            };
        }

        // ----------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------
        public async Task CreateAsync(CreateShiftWorkViewModel model, string user)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (model.FinalValue <= model.InitialValue)
                throw new InvalidOperationException("Final counter must be greater than the initial counter.");

            if (!await _db.Locomotives.AnyAsync(l => l.Id == model.LocomotiveId))
                throw new ArgumentException("Locomotive not found.");

            var e = new ShiftWork
            {
                LocomotiveId = model.LocomotiveId,
                Date = model.Date.Date,
                Shift = model.Shift,
                InitialValue = model.InitialValue,
                FinalValue = model.FinalValue,
                Amount = model.FinalValue - model.InitialValue,
                Note = model.Note,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user
            };

            _db.ShiftWorks.Add(e);
            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // EDIT
        // ----------------------------------------------------------
        public async Task EditAsync(int id, EditShiftWorkViewModel model, string user)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var e = await _db.ShiftWorks
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException("ShiftWork not found.");

            if (model.FinalValue <= model.InitialValue)
                throw new InvalidOperationException("Final counter must be greater than the initial counter.");

            e.LocomotiveId = model.LocomotiveId;
            e.Date = model.Date.Date;
            e.Shift = model.Shift;
            e.InitialValue = model.InitialValue;
            e.FinalValue = model.FinalValue;
            e.Amount = model.FinalValue - model.InitialValue;
            e.Note = model.Note;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // DELETE
        // ----------------------------------------------------------
        public async Task DeleteAsync(int id, string user)
        {
            var e = await _db.ShiftWorks
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException("ShiftWork not found.");

            e.IsDeleted = true;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // UNDELETE
        // ----------------------------------------------------------
        public async Task UndoDeleteAsync(int id, string user)
        {
            var e = await _db.ShiftWorks
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException("ShiftWork not found.");

            e.IsDeleted = false;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }
    }
}
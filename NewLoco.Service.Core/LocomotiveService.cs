using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core.Contracts;
using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core
{
    public class LocomotiveService : ILocomotiveService
    {
        private readonly LocoDbContext _db;

        public LocomotiveService(LocoDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // ----------------------------------------------------------
        // LIST WITH FILTER
        // ----------------------------------------------------------
        public async Task<IEnumerable<LocoNumberDto>> GetAllAsync(string? filter)
        {
            IQueryable<Locomotive> query = _db.Locomotives.AsQueryable();

            query = filter switch
            {
                "deleted" => query.IgnoreQueryFilters().Where(l => l.IsDeleted),
                "all" => query.IgnoreQueryFilters(),
                _ => query.Where(l => !l.IsDeleted)
            };

            return await query
                .AsNoTracking()
                .OrderBy(l => l.Number)
                .Select(l => new LocoNumberDto(
                    l.Id,
                    l.Number,
                    l.LocomotiveType,
                    l.MeasuringUnit,
                    l.AxlesCount,
                    l.IsDeleted,
                    l.Note))
                .ToListAsync();
        }

        // ----------------------------------------------------------
        // DETAILS
        // ----------------------------------------------------------
        public async Task<LocoDetailsDto> GetDetailsAsync(int id)
        {
            var e = await _db.Locomotives
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null)
                throw new ArgumentException("Locomotive not found.");

            return new LocoDetailsDto(
                e.Id,
                e.Number,
                e.LocomotiveType,
                e.MeasuringUnit,
                e.AxlesCount,
                e.Note ?? string.Empty,
                e.IsDeleted,
                e.CreatedOn,
                e.CreatedBy,
                e.ModifiedOn,
                e.ModifiedBy);
        }

        // ----------------------------------------------------------
        // LOAD FOR EDIT
        // ----------------------------------------------------------
        public async Task<LocomotiveFormDto?> GetForEditAsync(int id)
        {
            var e = await _db.Locomotives
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null)
                return null;

            return new LocomotiveFormDto(
                e.Number,
                e.LocomotiveType,
                e.MeasuringUnit,
                e.AxlesCount,
                e.Note
            );
        }

        // ----------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------
        public async Task CreateAsync(LocomotiveFormDto model, string user)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var e = new Locomotive
            {
                Number = model.Number,
                LocomotiveType = model.LocomotiveType,
                MeasuringUnit = model.MeasuringUnit,
                AxlesCount = model.AxlesCount,
                Note = model.Note,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user
            };

            _db.Locomotives.Add(e);
            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // EDIT
        // ----------------------------------------------------------
        public async Task EditAsync(int id, LocomotiveFormDto model, string user)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var e = await _db.Locomotives.FindAsync(id)
                ?? throw new ArgumentException("Locomotive not found.");

            e.Number = model.Number;
            e.LocomotiveType = model.LocomotiveType;
            e.MeasuringUnit = model.MeasuringUnit;
            e.AxlesCount = model.AxlesCount;
            e.Note = model.Note;

            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // SOFT DELETE
        // ----------------------------------------------------------
        public async Task DeleteAsync(int id, string user)
        {
            var e = await _db.Locomotives.FindAsync(id)
                ?? throw new ArgumentException("Locomotive not found.");

            e.IsDeleted = true;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // UNDELETE
        // ----------------------------------------------------------
        public async Task UndeleteAsync(int id, string user)
        {
            var e = await _db.Locomotives.FindAsync(id)
                ?? throw new ArgumentException("Locomotive not found.");

            e.IsDeleted = false;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ----------------------------------------------------------
        // OPTIONS DROPDOWN
        // ----------------------------------------------------------
        public async Task<IEnumerable<LocoOptionDto>> GetOptionsAsync()
        {
            return await _db.Locomotives
                .AsNoTracking()
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Number)
                .Select(l => new LocoOptionDto(
                    l.Id,
                    l.Number,
                    l.AxlesCount))
                .ToListAsync();
        }

        // ----------------------------------------------------------
        // GET TYPE ONLY
        // ----------------------------------------------------------
        public async Task<LocomotiveType> GetTypeAsync(int locomotiveId)
        {
            return await _db.Locomotives
                .AsNoTracking()
                .Where(l => l.Id == locomotiveId)
                .Select(l => l.LocomotiveType)
                .FirstOrDefaultAsync();
        }
    }
}
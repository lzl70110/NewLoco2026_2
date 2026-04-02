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
            _db = db;
        }

        // ---------------------------
        // LIST WITH FILTER (Active / Deleted / All)
        // ---------------------------
        public async Task<IEnumerable<LocoNumberDto>> GetAllAsync(string? filter)
        {
            IQueryable<Locomotive> query = _db.Locomotives.AsQueryable();

            // Ако имаш глобален EF Core query filter за IsDeleted,
            // IgnoreQueryFilters() позволява да върнеш deleted записи
            query = filter switch
            {
                "deleted" => query.IgnoreQueryFilters().Where(l => l.IsDeleted),
                "all" => query.IgnoreQueryFilters(),
                _ => query.Where(l => !l.IsDeleted)   // default = active
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

        // ---------------------------
        // DETAILS
        // ---------------------------
        public async Task<LocoDetailsDto?> GetDetailsAsync(int id)
        {
            var e = await _db.Locomotives
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null) return null;

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

        // ---------------------------
        // LOAD FOR EDIT
        // ---------------------------
        public async Task<LocomotiveFormDto?> GetForEditAsync(int id)
        {
            var e = await _db.Locomotives.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return null;

            return new LocomotiveFormDto(
                e.Number,
                e.LocomotiveType,
                e.MeasuringUnit,
                e.AxlesCount,
                e.Note
            );
        }

        // ---------------------------
        // CREATE
        // ---------------------------
        public async Task CreateAsync(LocomotiveFormDto model, string user)
        {
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

        // ---------------------------
        // EDIT
        // ---------------------------
        public async Task EditAsync(int id, LocomotiveFormDto model, string user)
        {
            var e = await _db.Locomotives.FindAsync(id);
            if (e == null) return;

            e.Number = model.Number;
            e.LocomotiveType = model.LocomotiveType;
            e.MeasuringUnit = model.MeasuringUnit;
            e.AxlesCount = model.AxlesCount;
            e.Note = model.Note;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ---------------------------
        // SOFT DELETE
        // ---------------------------
        public async Task DeleteAsync(int id, string user)
        {
            var e = await _db.Locomotives.FindAsync(id);
            if (e == null || e.IsDeleted) return;

            e.IsDeleted = true;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ---------------------------
        // UNDELETE
        // ---------------------------
        public async Task UndeleteAsync(int id, string user)
        {
            var e = await _db.Locomotives.FindAsync(id);
            if (e == null || !e.IsDeleted) return;

            e.IsDeleted = false;
            e.ModifiedOn = DateTime.UtcNow;
            e.ModifiedBy = user;

            await _db.SaveChangesAsync();
        }

        // ---------------------------
        // DROPDOWN OPTIONS
        // ---------------------------
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

        // ---------------------------
        // GET TYPE
        // ---------------------------
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
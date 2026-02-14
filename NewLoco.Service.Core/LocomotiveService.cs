using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Locomotives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewLoco.Service.Core
    {
    public class LocomotiveService : ILocomotiveService
        {
        private readonly LocoDbContext _db;

        public LocomotiveService(LocoDbContext db)
            {
            _db = db;
            }

        public async Task<IEnumerable<LocomotiveNumberViewModel>> GetAll(string filter)
            {
            var query = _db.Locomotives.AsQueryable();

            query = filter switch
                {
                    "deleted" => query.Where(l => l.IsDeleted),
                    "all" => query,
                    _ => query.Where(l => !l.IsDeleted)
                    };

            return await query
                .OrderBy(l => l.Number)
                .Select(l => new LocomotiveNumberViewModel
                    {
                    Id = l.Id,
                    Number = l.Number,
                    LocomotiveType = l.LocomotiveType,
                    MeasuringUnit = l.MeasuringUnit,
                    IsDeleted = l.IsDeleted
                    })
                .ToListAsync();
            }

        public async Task<LocomotiveDetailsViewModel?> GetDetails(int id)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return null;

            return new LocomotiveDetailsViewModel
                {
                Id = entity.Id,
                Number = entity.Number,
                LocomotiveType = entity.LocomotiveType,
                MeasuringUnit = entity.MeasuringUnit,
                Note = entity.Note ?? string.Empty,
                IsDeleted = entity.IsDeleted,
                CreatedOn = entity.CreatedOn,
                CreatedBy = entity.CreatedBy,
                ModifiedOn = entity.ModifiedOn,
                ModifiedBy = entity.ModifiedBy
                };
            }

        public async Task<LocomotiveFormModel?> GetForEdit(int id)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return null;

            return new LocomotiveFormModel
                {
                Number = entity.Number,
                LocomotiveType = entity.LocomotiveType,
                MeasuringUnit = entity.MeasuringUnit,
                Note = entity.Note
                };
            }

        public async Task CreateAsync(LocomotiveFormModel model, string user)
            {
            var entity = new Locomotive
                {
                Number = model.Number,
                LocomotiveType = model.LocomotiveType,
                MeasuringUnit = model.MeasuringUnit,
                Note = model.Note,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user
                };

            _db.Locomotives.Add(entity);
            await _db.SaveChangesAsync();
            }

        public async Task EditAsync(int id, LocomotiveFormModel model, string user)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return;

            entity.Number = model.Number;
            entity.LocomotiveType = model.LocomotiveType;
            entity.MeasuringUnit = model.MeasuringUnit;
            entity.Note = model.Note;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = user;

            await _db.SaveChangesAsync();
            }

        public async Task DeleteAsync(int id, string user)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = user;

            await _db.SaveChangesAsync();
            }

        public async Task UndeleteAsync(int id, string user)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return;

            entity.IsDeleted = false;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = user;

            await _db.SaveChangesAsync();
            }
        }
    }

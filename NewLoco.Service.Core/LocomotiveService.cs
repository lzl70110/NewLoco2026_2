using System.Linq;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core.Contracts;
using static NewLoco.Service.Core.Contracts.LocomotiveDtos;
using  NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core;

public class LocomotiveService : ILocomotiveService
    {
    private readonly LocoDbContext _db;

    public LocomotiveService(LocoDbContext db)
        {
        _db = db;
        }

    // List with filter: "deleted" | "all" | default (only active)
    public async Task<IEnumerable<LocoNumberDto>> GetAllAsync(string? filter)
        {
        var query = _db.Locomotives.AsQueryable();

        query = filter switch
            {
                "deleted" => query.Where(l => l.IsDeleted),
                "all" => query,
                _ => query.Where(l => !l.IsDeleted)
                };

        return await query
            .AsNoTracking()
            .OrderBy(l => l.Number)
            .Select(l => new LocoNumberDto(
                l.Id, l.Number,
                l.LocomotiveType,
                l.MeasuringUnit, l.IsDeleted))
            .ToListAsync();
        }

    // Details
    public async Task<LocoDetailsDto?> GetDetailsAsync(int id)
        {
        var e = await _db.Locomotives.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return null;

        return new LocoDetailsDto(
            e.Id,
            e.Number, 
            e.LocomotiveType,
            e.MeasuringUnit,
            e.Note ?? string.Empty,
            e.IsDeleted,
            e.CreatedOn, 
            e.CreatedBy, 
            e.ModifiedOn,
            e.ModifiedBy);
        }

    // Load for edit
    public async Task<LocomotiveFormDto?> GetForEditAsync(int id)
        {
        var e = await _db.Locomotives.FindAsync(id);
        if (e == null) return null;

        return new LocomotiveFormDto(e.Number, e.LocomotiveType, e.MeasuringUnit, e.Note);
        }

    // Create
    public async Task CreateAsync(LocomotiveFormDto model, string user)
        {
        var e = new Locomotive
            {
            Number = model.Number,
            LocomotiveType = model.LocomotiveType,
            MeasuringUnit = model.MeasuringUnit,
            Note = model.Note,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = user
            };

        _db.Locomotives.Add(e);
        await _db.SaveChangesAsync();
        }

    // Edit
    public async Task EditAsync(int id, LocomotiveFormDto model, string user)
        {
        var e = await _db.Locomotives.FindAsync(id);
        if (e == null) return;

        e.Number = model.Number;
        e.LocomotiveType = model.LocomotiveType;
        e.MeasuringUnit = model.MeasuringUnit;
        e.Note = model.Note;
        e.ModifiedOn = DateTime.UtcNow;
        e.ModifiedBy = user;

        await _db.SaveChangesAsync();
        }

    // Soft-delete
    public async Task DeleteAsync(int id, string user)
        {
        var e = await _db.Locomotives.FindAsync(id);
        if (e == null || e.IsDeleted) return;

        e.IsDeleted = true;
        e.ModifiedOn = DateTime.UtcNow;
        e.ModifiedBy = user;

        await _db.SaveChangesAsync();
        }

    // Undo soft-delete
    public async Task UndeleteAsync(int id, string user)
        {
        var e = await _db.Locomotives.FindAsync(id);
        if (e == null || !e.IsDeleted) return;

        e.IsDeleted = false;
        e.ModifiedOn = DateTime.UtcNow;
        e.ModifiedBy = user;

        await _db.SaveChangesAsync();
        }

    // Options for dropdowns (DTO)
    public async Task<IEnumerable<LocoOptionDto>> GetOptionsAsync()
        => await _db.Locomotives
            .AsNoTracking()
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.Number)
            .Select(l => new LocoOptionDto(l.Id, l.Number))
            .ToListAsync();
    }
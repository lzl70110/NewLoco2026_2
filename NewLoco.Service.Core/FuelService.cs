using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Service.Core.Services;

public class FuelService : IFuelService
    {
    private readonly LocoDbContext context;

    public FuelService(LocoDbContext context)
        {
        this.context = context;
        }

    public IEnumerable<FuelAllViewModel> GetAll()
        {
        return context.Fuels
            .AsNoTracking()
            .Include(f => f.Locomotive)
            .Select(f => new FuelAllViewModel
                {
                Id = f.Id,
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

    public CreateFuelViewModel CreateModel()
        => new CreateFuelViewModel
            {
            Date = DateTime.Now,
            InitialFuel = 0
            };

    public async Task CreateAsync(CreateFuelViewModel model, string user)
        {
        var fuel = new Fuel
            {
            LocoId = model.LocomotiveId,
            Date = model.Date ?? DateTime.Now,
            InitialFuel = model.InitialFuel,
            FinalFuel = model.FinalFuel,
            Refueled = model.Refueled,
            Note = model.Note ?? "",
            CreatedBy = user,
            CreatedOn = DateTime.Now,
            Consumption = (model.InitialFuel + model.Refueled) - model.FinalFuel
            };

        context.Fuels.Add(fuel);
        await context.SaveChangesAsync();
        }

    public FuelAllViewModel? GetForEdit(int id)
        {
        return context.Fuels
            .Include(f => f.Locomotive)
            .Where(f => f.Id == id)
            .Select(f => new FuelAllViewModel
                {
                Id = f.Id,
                LocomotiveNumber = f.Locomotive.Number,
                Date = f.Date,
                InitialFuel = f.InitialFuel,
                FinalFuel = f.FinalFuel,
                Consumption = f.Consumption,
                Refueled = f.Refueled,
                Note = f.Note ?? ""
                })
            .FirstOrDefault();
        }

    public async Task EditAsync(int id, FuelAllViewModel model, string user)
        {
        var fuel = await context.Fuels.FindAsync(id);
        if (fuel == null) return;

        fuel.InitialFuel = model.InitialFuel;
        fuel.FinalFuel = model.FinalFuel;
        fuel.Refueled = model.Refueled;
        fuel.Note = model.Note ?? "";
        fuel.Consumption = (model.InitialFuel + model.Refueled) - model.FinalFuel;
        fuel.ModifiedBy = user;
        fuel.ModifiedOn = DateTime.Now;

        await context.SaveChangesAsync();
        }

    public async Task DeleteAsync(int id, string user)
        {
        var fuel = await context.Fuels.FindAsync(id);
        if (fuel == null) return;

        fuel.IsDeleted = true;
        fuel.ModifiedBy = user;
        fuel.ModifiedOn = DateTime.Now;

        await context.SaveChangesAsync();
        }

    public async Task UndoDeleteAsync(int id, string user)
        {
        var fuel = await context.Fuels.FindAsync(id);
        if (fuel == null) return;

        fuel.IsDeleted = false;
        fuel.ModifiedBy = user;
        fuel.ModifiedOn = DateTime.Now;

        await context.SaveChangesAsync();
        }

    public decimal GetLastFuel(int locomotiveId)
        {
        return context.Fuels
            .Where(f => f.LocoId == locomotiveId)
            .OrderByDescending(f => f.Date)
            .Select(f => f.FinalFuel)
            .FirstOrDefault();
        }
    }

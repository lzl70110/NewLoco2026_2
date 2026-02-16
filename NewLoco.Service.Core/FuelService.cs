using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Fuels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewLoco.Service.Core.Services
    {
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
            var date = model.Date;
            
            var prevFinal = await context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == model.LocomotiveId && !f.IsDeleted && f.Date < date)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => (decimal?)f.FinalFuel)
                .FirstOrDefaultAsync();

            var initial = prevFinal ?? 0m;
            var consumption = (initial + model.Refueled) - model.FinalFuel;

            var fuel = new Fuel
                {
                LocoId = model.LocomotiveId,
                Date = date,
                InitialFuel = initial,
                FinalFuel = model.FinalFuel,
                Refueled = model.Refueled,
                Note = model.Note ?? string.Empty,
                CreatedBy = user,
                CreatedOn = DateTime.Now,
                Consumption = consumption
                };
            
            context.Fuels.Add(fuel);
            await context.SaveChangesAsync();
            }

        public FuelAllViewModel? GetForEdit(int id)
            {
            return context.Fuels
                .AsNoTracking()
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

            var date = model.Date;

            var prevFinal = await context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == fuel.LocoId && !f.IsDeleted && f.Id != id && f.Date < date)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => (decimal?)f.FinalFuel)
                .FirstOrDefaultAsync();

            fuel.Date = date;

            if (prevFinal.HasValue)
                fuel.InitialFuel = prevFinal.Value;   // има предходна смяна → взимаме нейното FinalFuel
            // иначе: няма предходна смяна → запазваме текущото InitialFuel (не го нулираме)

            fuel.FinalFuel = model.FinalFuel;
            fuel.Refueled = model.Refueled;
            fuel.Note = model.Note ?? string.Empty;
            fuel.Consumption = (fuel.InitialFuel + fuel.Refueled) - fuel.FinalFuel;
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
                .AsNoTracking()
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefault();
            }

        public async Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date)
            {
            return await context.Fuels
                .AsNoTracking()
                .Where(f => f.LocoId == locomotiveId && !f.IsDeleted && f.Date < date)
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync();
            }
        }
    }
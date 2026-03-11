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
using static GCommon.Messages;

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
                    Note = f.Note ?? string.Empty,
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
                Date = DateTime.Today,
                InitialFuel = 0
            };

        public async Task CreateAsync(CreateFuelViewModel model, string user)
        {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException(FuelServiceKeys.Msg_FuelInFuture); // replaced literal with key

            var prevFinal = await GetPrevFinalAsync(model.LocomotiveId, model.Date);
            var initial = prevFinal;

            if (model.FinalFuel > initial + model.Refueled)
                throw new InvalidOperationException(FuelServiceKeys.Msg_FinalFuelTooHigh); // replaced literal with key

            var consumption = (initial + model.Refueled) - model.FinalFuel;

            var fuel = new Fuel
            {
                // NOTE: Using LocomotiveId (matches model Fuel)
                Id = model.LocomotiveId,
                Date = model.Date,
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
                    Note = f.Note ?? string.Empty
                })
                .FirstOrDefault();
        }

        public async Task EditAsync(int id, FuelAllViewModel model, string user)
        {
            if (model.Date.Date > DateTime.Today)
                throw new InvalidOperationException(FuelServiceKeys.Msg_FuelInFuture);  

            var fuel = await context.Fuels.FindAsync(id);
            if (fuel == null) return;

            var prevFinal = await GetPrevFinalAsync(fuel.Id, model.Date);  
            fuel.InitialFuel = prevFinal;

            if (model.FinalFuel > fuel.InitialFuel + model.Refueled)
                throw new InvalidOperationException(FuelServiceKeys.Msg_FinalFuelTooHigh);  

            fuel.Date = model.Date;
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
                .Where(f => f.Id == locomotiveId && !f.IsDeleted)  
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefault();
        }

        public async Task<decimal> GetPrevFinalAsync(int locomotiveId, DateTime date)
        {
            return await context.Fuels
                .AsNoTracking()
                .Where(f => f.Id == locomotiveId && !f.IsDeleted && f.Date < date) 
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .Select(f => f.FinalFuel)
                .FirstOrDefaultAsync();
        }

        // Consume fuel for work: cannot consume more than available
        public async Task ConsumeFuelAsync(int locomotiveId, decimal amount, string user)
        {
            if (amount <= 0) return;

            var lastFuel = await context.Fuels
                .Where(f => f.Id == locomotiveId && !f.IsDeleted)  
                .OrderByDescending(f => f.Date).ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            if (lastFuel == null)
                throw new InvalidOperationException(FuelServiceKeys.Msg_NoFuelRecordForLoco);  
            var available = lastFuel.FinalFuel;

            if (amount > available)
                throw new InvalidOperationException(FuelServiceKeys.Msg_NotEnoughFuel);  

            lastFuel.FinalFuel -= amount;
            lastFuel.Consumption += amount;
            lastFuel.ModifiedBy = user;
            lastFuel.ModifiedOn = DateTime.Now;

            await context.SaveChangesAsync();
        }
    }
}
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Axles;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Service.Core
{
    public class AxleMeasurementService(LocoDbContext context) : IAxleMeasurementService
    {
        private readonly LocoDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        // --------------------------------------------------------
        // LIST
        // --------------------------------------------------------
        public async Task<List<AxleMeasurementCardListViewModel>> GetAllAsync()
        {
            return await _context.AxleMeasurementCards
                .Include(c => c.Locomotive)
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new AxleMeasurementCardListViewModel
                {
                    Id = c.Id,
                    DocumentNumber = c.DocumentNumber,
                    LocomotiveNumber = c.Locomotive.Number,
                    MeasurementDate = c.MeasurementDate,
                    AxleCount = c.AxleCount,
                    IsDeleted= c.IsDeleted
                })
                .ToListAsync();
        }

        // --------------------------------------------------------
        // DETAILS
        // --------------------------------------------------------
        public async Task<AxleMeasurementCardDetailsViewModel> GetDetailsAsync(int id)
        {
            var card = await _context.AxleMeasurementCards
                .Include(c => c.Locomotive)
                .Include(c => c.Axles)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new ArgumentException("Card not found.");

            var model = new AxleMeasurementCardDetailsViewModel
            {
                Id = card.Id,
                DocumentNumber = card.DocumentNumber,
                LocomotiveNumber = card.Locomotive.Number,
                MeasurementDate = card.MeasurementDate,
                AxleCount = card.AxleCount,
                CreatedBy = card.CreatedBy,
                CreatedOn = card.CreatedOn
            };

            foreach (var axle in card.Axles.OrderBy(a => a.AxleNumber))
            {
                model.Axles.Add(new AxleMeasurementValueDetailsViewModel
                {
                    AxleNumber = axle.AxleNumber,
                    QR_Left = axle.qR_Left,
                    QR_Right = axle.qR_Right,
                    Sd_Left = axle.Sd_Left,
                    Sd_Right = axle.Sd_Right,
                    Sh_Left = axle.Sh_Left,
                    Sh_Right = axle.Sh_Right,
                    Ar = axle.Ar,
                    Sr = axle.Sr
                });
            }

            return model;
        }

        // --------------------------------------------------------
        // GET CREATE
        // --------------------------------------------------------
        public async Task<AxleMeasurementCardViewModel> GetCreateModelAsync()
        {
            var locos = await _context.Locomotives
                .OrderBy(l => l.Number)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Number
                })
                .ToListAsync();

            return new AxleMeasurementCardViewModel
            {
                Locomotives = locos,
                Axles = new List<AxleMeasurementValueViewModel>() // empty, filled by AJAX
            };
        }

        // --------------------------------------------------------
        // CREATE (POST)
        // --------------------------------------------------------
        public async Task<int> CreateAsync(AxleMeasurementCardViewModel model, string createdBy)
        {
            ArgumentNullException.ThrowIfNull(model);

            CalculateSr(model);

            int year = DateTime.UtcNow.Year;
            int? last = await _context.AxleMeasurementCards
                .Where(c => c.Year == year)
                .MaxAsync(c => (int?)c.SequenceNumber);

            int nextSequence = (last ?? 0) + 1;

            var card = new AxleMeasurementCard
            {
                SelectedLocomotiveId = model.SelectedLocomotiveId,
                MeasurementDate = model.MeasurementDate,
                AxleCount = model.Axles.Count,
                Year = year,
                SequenceNumber = nextSequence,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy,
                Axles = model.Axles
                    .Where(a =>
                        a.Ar.HasValue ||
                        a.Sd_Left.HasValue ||
                        a.Sd_Right.HasValue ||
                        a.Sh_Left.HasValue ||
                        a.Sh_Right.HasValue ||
                        a.QR_Left.HasValue ||
                        a.QR_Right.HasValue)
                    .OrderBy(a => a.AxleNumber)
                    .Select(a => new AxleMeasurementValue
                    {
                        AxleNumber = a.AxleNumber,
                        Sd_Left = a.Sd_Left ?? 0,
                        Sd_Right = a.Sd_Right ?? 0,
                        Sh_Left = a.Sh_Left ?? 0,
                        Sh_Right = a.Sh_Right ?? 0,
                        qR_Left = a.QR_Left ?? 0,
                        qR_Right = a.QR_Right ?? 0,
                        Ar = a.Ar ?? 0,
                        Sr = a.Sr ?? 0
                    })
                    .ToList()
            };

            _context.AxleMeasurementCards.Add(card);
            await _context.SaveChangesAsync();

            return card.Id;
        }

        // --------------------------------------------------------
        // GET EDIT
        // --------------------------------------------------------
        public async Task<AxleMeasurementCardViewModel> GetEditModelAsync(int id)
        {
            var card = await _context.AxleMeasurementCards
                .Include(c => c.Axles)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new ArgumentException("Card not found.");

            return new AxleMeasurementCardViewModel
            {
                Id = card.Id,
                SelectedLocomotiveId = card.SelectedLocomotiveId,
                MeasurementDate = card.MeasurementDate,
                Axles = card.Axles
                    .OrderBy(a => a.AxleNumber)
                    .Select(a => new AxleMeasurementValueViewModel
                    {
                        AxleNumber = a.AxleNumber,
                        Sd_Left = a.Sd_Left,
                        Sd_Right = a.Sd_Right,
                        Sh_Left = a.Sh_Left,
                        Sh_Right = a.Sh_Right,
                        QR_Left = a.qR_Left,
                        QR_Right = a.qR_Right,
                        Ar = a.Ar,
                        Sr = a.Sr
                    })
                    .ToList()
            };
        }

        // --------------------------------------------------------
        // UPDATE (POST)
        // --------------------------------------------------------
        public async Task UpdateAsync(AxleMeasurementCardViewModel model, string modifiedBy)
        {
            ArgumentNullException.ThrowIfNull(model);

            var card = await _context.AxleMeasurementCards
                .Include(c => c.Axles)
                .FirstOrDefaultAsync(c => c.Id == model.Id)
                ?? throw new ArgumentException("Card not found.");

            card.SelectedLocomotiveId = model.SelectedLocomotiveId;
            card.MeasurementDate = model.MeasurementDate;
            card.AxleCount = model.Axles.Count;
            card.ModifiedOn = DateTime.UtcNow;
            card.ModifiedBy = modifiedBy;

            card.Axles.Clear();

            CalculateSr(model);

            card.Axles = model.Axles
                .Where(a =>
                    a.Ar.HasValue ||
                    a.Sd_Left.HasValue ||
                    a.Sd_Right.HasValue ||
                    a.Sh_Left.HasValue ||
                    a.Sh_Right.HasValue ||
                    a.QR_Left.HasValue ||
                    a.QR_Right.HasValue)
                .OrderBy(a => a.AxleNumber)
                .Select(a => new AxleMeasurementValue
                {
                    AxleNumber = a.AxleNumber,
                    Sd_Left = a.Sd_Left ?? 0,
                    Sd_Right = a.Sd_Right ?? 0,
                    Sh_Left = a.Sh_Left ?? 0,
                    Sh_Right = a.Sh_Right ?? 0,
                    qR_Left = a.QR_Left ?? 0,
                    qR_Right = a.QR_Right ?? 0,
                    Ar = a.Ar ?? 0,
                    Sr = a.Sr ?? 0
                })
                .ToList();

            await _context.SaveChangesAsync();
        }

        // --------------------------------------------------------
        // AJAX helper (NEW)
        // --------------------------------------------------------
        public async Task<int> GetAxlesCountAsync(int locoId)
        {
            return await _context.Locomotives
                .Where(l => l.Id == locoId)
                .Select(l => l.AxlesCount)
                .FirstAsync();
        }

        // --------------------------------------------------------
        // SR CALCULATION
        // --------------------------------------------------------
        public void CalculateSr(AxleMeasurementCardViewModel model)
        {
            if (model?.Axles == null)
                return;

            foreach (var ax in model.Axles)
            {
                if (ax.Ar.HasValue &&
                    ax.Sd_Left.HasValue &&
                    ax.Sd_Right.HasValue)
                {
                    ax.Sr = ax.Ar.Value + ax.Sd_Left.Value + ax.Sd_Right.Value;
                }
            }
        }

        public IQueryable<AxleMeasurementCard> Query()
             => _context.AxleMeasurementCards
             .Include(c => c.Locomotive)
             .AsQueryable();

        public async Task DeleteAsync(int id)
        {
            var card = await _context.AxleMeasurementCards.FindAsync(id);
            if (card == null) return;
            card.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(int id)
        {
            var card = await _context.AxleMeasurementCards.FindAsync(id);
            if (card == null) return;
            card.IsDeleted = false;
            await _context.SaveChangesAsync();
        }
    }
}
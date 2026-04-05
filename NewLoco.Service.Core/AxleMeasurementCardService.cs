using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Web.ViewModels.Axles;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Service.Core
{
    public class AxleMeasurementCardService
    {
        private readonly LocoDbContext _context;

        public AxleMeasurementCardService(LocoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AxleMeasurementCardViewModel> GetCreateModelAsync()
        { 
            var locos = await _context.Locomotives
                .OrderBy(x => x.Number)
                .Select(x => new
                {
                    x.Id,
                    x.Number
                })
                .ToListAsync();

            var locosSelectList = locos
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Number
                })
                .ToList();

            var model = new AxleMeasurementCardViewModel
            {
                Locomotives = locosSelectList,
                Axles = new List<AxleMeasurementValueViewModel>()
            };

            return model;
        }
        public async Task DeleteAsync(int id)
        {
            var card = await _context.AxleMeasurementCards.FindAsync(id);
            if (card != null)
            {
                card.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(int id)
        {
            var card = await _context.AxleMeasurementCards.FindAsync(id);
            if (card != null)
            {
                card.IsDeleted = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
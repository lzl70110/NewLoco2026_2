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
            // Взимаме локомотивите
            var locos = await _context.Locomotives
                .OrderBy(x => x.Number)
                .Select(x => new
                {
                    x.Id,
                    x.Number
                })
                .ToListAsync();

            // Мапваме към SelectListItem
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
    }
}
using System.Collections.Generic;
using NewLoco.Web.ViewModels.Paging;

namespace NewLoco.Web.ViewModels.Axles
{
    public class AxleMeasurementIndexViewModel
    {
        public AxleMeasurementFilterViewModel Filter { get; set; }
            = new AxleMeasurementFilterViewModel();

        public PagingInfo Paging { get; set; }
            = new PagingInfo { PageNumber = 1, PageSize = 20, TotalItems = 0 };

        public IEnumerable<AxleMeasurementListItemViewModel> Items { get; set; }
            = new List<AxleMeasurementListItemViewModel>();
    }
}
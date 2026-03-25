using System.Collections.Generic;
using NewLoco.Web.ViewModels.Paging;

namespace NewLoco.Web.ViewModels.ShiftWorks
{
    public sealed class ShiftWorkIndexViewModel
    {
        public IReadOnlyList<ShiftWorkListItemViewModel> Items { get; init; } = [];
        public ShiftWorkFilterInput Filter { get; init; } = new();
        public PagingInfo Paging { get; init; } = new();
    }
}
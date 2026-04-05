using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using GCommon.Enums;
using NewLoco.Web.ViewModels.Paging;

namespace NewLoco.Web.ViewModels.ShiftWorks
{
    public sealed class ShiftWorkIndexViewModel
    {
        public IReadOnlyList<ShiftWorkListItemViewModel> Items { get; init; } = [];
        public ShiftWorkFilterInput Filter { get; init; } = new();
        public PagingInfo Paging { get; init; } = new();

        public SelectList ShowModeOptions => new SelectList(
            new[]
            {
                new { Text = "Active", Value = ShiftWorkShowMode.Active.ToString() },
                new { Text = "Deleted", Value = ShiftWorkShowMode.Deleted.ToString() },
                new { Text = "All", Value = ShiftWorkShowMode.All.ToString() }
            },
            "Value",
            "Text",
            Filter.ShowMode.ToString() 
        );
    }
}
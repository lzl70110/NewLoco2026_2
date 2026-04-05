using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewLoco.Web.ViewModels.Axles
{
    public class AxleMeasurementCardViewModel
    {
        public int Id { get; set; }
        public DateTime MeasurementDate { get; set; } = DateTime.Today;

        public int SelectedLocomotiveId { get; set; }

        public List<SelectListItem> Locomotives { get; set; } = new();

        public int AxlesCount { get; set; }

        public List<AxleMeasurementValueViewModel> Axles { get; set; } = new();
    }
}
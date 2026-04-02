using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewLoco.Web.ViewModels.Axles
{
    public class AxleMeasurementCardViewModel
    {
        // Primary key (използва се при Edit)
        public int Id { get; set; }

        // Дата на измерване
        [Display(Name = "Дата на измерване")]
        public DateTime MeasurementDate { get; set; } = DateTime.Today;

        // Избран локомотив от dropdown
        [Display(Name = "Локомотив")]
        public int SelectedLocomotiveId { get; set; }

        // Dropdown list
        public List<SelectListItem> Locomotives { get; set; } = new();

        // Колко оси има избраният локомотив (за динамика)
        public int AxlesCount { get; set; }

        // Осите с измерванията
        public List<AxleMeasurementValueViewModel> Axles { get; set; } = new();
    }
}
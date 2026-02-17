using System;
using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.ShiftWork;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;

namespace NewLoco.Web.ViewModels.ShiftWorks
    {
    public class EditShiftWorkViewModel : ShiftWorksViewModelBase
        {
        public int Id { get; set; }

        // decimal вместо int
        public decimal StartValue
            {
            get => InitialValue;
            set => InitialValue = value;
            }

        public decimal EndValue
            {
            get => FinalValue;
            set => FinalValue = value;
            }

        // Display-only, не се присвоява
        public string Locomotive
            {
            get => Locomotives.FirstOrDefault(l => l.Value == LocomotiveId.ToString())?.Text ?? "";
            }
        }
    }

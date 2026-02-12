using System;
using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;

namespace NewLoco.Data.Models
    {
    public abstract class BaseEntity
        {
        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        [StringLength(CreatedByMaxLength)]
        public string CreatedBy { get; set; } = null!;

        [StringLength(ModifiedByMaxLength)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        [StringLength(NoteMaxLength)]
        public string? Note { get; set; }
        }
    }

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudServise_API.Models
{
    public class Discipline
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string CreatorId { get; set; }

        public List<DisciplineGroupTeacher> DisciplineGroupTeachers { get; set; }

        //public List<LaboratoryWork> Laboratories { get; set; }
    }
}

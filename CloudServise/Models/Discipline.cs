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

        public Discipline() {}
        public Discipline(string name, string creatorId)
        {
            Name = name;
            CreatorId = creatorId;
        }

        public DisciplineDTO ToDisciplineDto()
        {
            DisciplineDTO disciplineDto = new DisciplineDTO(Id, Name, CreatorId);
            return disciplineDto;
        }
    }

    public class DisciplineDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CreatorId { get; set; }

        public DisciplineDTO() {}
        public DisciplineDTO(Guid id, string name, string creatorId)
        {
            Id = id;
            Name = name;
            CreatorId = creatorId;
        }
    }
}

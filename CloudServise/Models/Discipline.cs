using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Models
{
    public class Discipline
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        public List<DisciplineGroupTeacher> DisciplineGroupTeachers { get; set; }

        public List<LaboratoryWork> Laboratories { get; set; }

        public Discipline() {}
        public Discipline(string name, Guid ownerId, string shortName)
        {
            Name = name;
            OwnerId = ownerId;
            ShortName = shortName;
        }

        public DisciplineDTO ToDisciplineDto()
        {
            DisciplineDTO disciplineDto = new DisciplineDTO(Id, Name, OwnerId, ShortName);
            return disciplineDto;
        }
    }

    public class DisciplineDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Guid OwnerId { get; set; }

        public DisciplineDTO() {}
        public DisciplineDTO(Guid id, string name, Guid ownerId, string shortName)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            ShortName = shortName;
        }
    }

    public class CreateDisciplineDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}

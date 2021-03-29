using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Models
{
    public class LaboratoryWork
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerId { get; set; }

        public Guid DisciplineId { get; set; }
        [ForeignKey("DisciplineId")]
        public Discipline Discipline { get; set; }
        
        public LaboratoryWork() {}
        public LaboratoryWork(Guid id, string name, Guid ownerId, Guid disciplineId)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            DisciplineId = disciplineId;
        }

        public LaboratoryWorkDTO ToLaboratoryWorkDto()
        {
            return new LaboratoryWorkDTO(Id, Name, OwnerId, DisciplineId);
        }
    }

    public class LaboratoryWorkDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerId { get; set; }
        public Guid DisciplineId { get; set; }

        public LaboratoryWorkDTO() {}
        public LaboratoryWorkDTO(Guid id, string name, Guid ownerId, Guid disciplineId)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            DisciplineId = disciplineId;
        }
    }

    public class CreateLaboratoryWorkDTO
    {
        public string Name { get; set; }
        public Guid DisciplineId { get; set; }
    }
}

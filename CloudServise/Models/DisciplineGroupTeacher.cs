using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudServise_API.Models
{
    public class DisciplineGroupTeacher
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid DisciplineId { get; set; }
        [ForeignKey("DisciplineId")]
        public Discipline Discipline { get; set; }

        [Required]
        public Guid GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        [Required]
        public Guid TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }

        public DisciplineGroupTeacher() {}
        public DisciplineGroupTeacher(Guid disciplineId, Guid groupId, Guid teacherId)
        {
            DisciplineId = disciplineId;
            GroupId = groupId;
            TeacherId = teacherId;
        }

        public DisciplineGroupTeacherDTO ToDisciplineGroupTeacherDto()
        {
            return new DisciplineGroupTeacherDTO(Id, DisciplineId, GroupId, TeacherId);
        }
    }


    public class DisciplineGroupTeacherDTO
    {
        public Guid Id { get; set; }
        public Guid DisciplineId { get; set; }
        public Guid GroupId { get; set; }
        public Guid TeacherId { get; set; }

        public DisciplineGroupTeacherDTO() {}
        public DisciplineGroupTeacherDTO(Guid id, Guid disciplineId, Guid groupId, Guid teacherId)
        {
            Id = id;
            DisciplineId = disciplineId;
            GroupId = groupId;
            TeacherId = teacherId;
        }
    }
}

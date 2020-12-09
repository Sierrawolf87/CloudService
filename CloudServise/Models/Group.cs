using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Models
{
    public class Group
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(5)]
        public string Name { get; set; }

        public List<DisciplineGroupTeacher> DisciplineGroupTeachers { get; set; }

        public List<User> Users { get; set; }

        public Group() {}
        public Group(string name)
        {
            Name = name;
        }

        public GroupDTO ToGroupDto()
        {
            return new GroupDTO(Id, Name);
        }
    }

    public class GroupDTO
    {
        public Guid Id;
        public string Name;

        public GroupDTO() { }
        public GroupDTO(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

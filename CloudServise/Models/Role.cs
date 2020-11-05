using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudServise_API.Models
{
    public class Role
    {
        [Key] 
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<User> Users { get; set; }

        public Role() {}
        public Role(string name)
        {
            Name = name;
        }

        public RoleDTO ToRoleDTO()
        {
            RoleDTO roleDto = new RoleDTO(Id, Name);
            return roleDto;
        }
    }

    public class RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public RoleDTO() {}
        public RoleDTO(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

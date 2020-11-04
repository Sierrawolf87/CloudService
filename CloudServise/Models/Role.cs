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
    }
}

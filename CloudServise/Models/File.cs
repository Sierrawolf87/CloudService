using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudServise_API.Models
{
    public class File
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Patch { get; set; }
        public Guid OwnerId { get; set; }

        public File() {}
        public File(string name, Guid ownerId, string LaboratoryWorkId)
        {
            Name = name;
            OwnerId = ownerId;
            Patch = $"{ownerId}/{LaboratoryWorkId}/{name}";
        }

        public FileDTO ToFileDto()
        {
            return new FileDTO(Id, Name, OwnerId, Patch);
        }
    }

    public class FileDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Patch { get; set; }
        public Guid OwnerId { get; set; }
        
        public FileDTO() {}
        public FileDTO(Guid id, string name, Guid ownerId, string patch)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            Patch = patch;
        }
    }
}

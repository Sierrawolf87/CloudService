using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CloudService_API.Data;
using Microsoft.AspNetCore.Http;

namespace CloudService_API.Models
{
    public class File
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PathToFile { get; set; }
        public string PathToDirectory { get; set; }
        public Guid OwnerId { get; set; }

        public Requirement Requirement { get; set; }

        public Solution Solution { get; set; }

        public File() {}

        // Конструктор для создания условий лабораторной работы
        public File(string name, Guid ownerId, Requirement requirement, string pathToDirectory)
        {
            Name = name;
            OwnerId = ownerId;
            Requirement = requirement;
            Guid laboratoryWorkId = requirement.LaboratoryWorkId;
            PathToFile = @$"{pathToDirectory}\{ownerId}\{laboratoryWorkId}\{Guid.NewGuid()}{Auxiliary.GetExtension(name)}";
            PathToDirectory = @$"{pathToDirectory}\{OwnerId}\{laboratoryWorkId}";
        }

        // Констроктор для создания решения от студента
        public File(string name, Guid ownerId, Solution solution, string pathToDirectory)
        {
            Name = name;
            OwnerId = ownerId;
            Solution = solution;
            Guid laboratoryWorkId = solution.LaboratoryWorkId;
            PathToFile = @$"{pathToDirectory}\{ownerId}\{laboratoryWorkId}\{Guid.NewGuid()}{Auxiliary.GetExtension(name)}";
            PathToDirectory = @$"{pathToDirectory}\{OwnerId}\{laboratoryWorkId}";
        }

        public FileDTO ToFileDto()
        {
            return new FileDTO(Id, Name, OwnerId, Requirement, Solution);
        }
    }

    public class FileDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid OwnerId { get; set; }

        public Guid? RequirementId { get; set; }
        public Guid? SolutionId { get; set; }

        public FileDTO() {}
        public FileDTO(Guid id, string name, Guid ownerId, Requirement requirement, Solution solution)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            if (requirement != null)
                RequirementId = requirement.Id;
            if (solution != null)
                SolutionId = solution.Id;
        }
    }

    public class FileUploadDTO
    {
        public RequirementDTO Requirement { get; set; }
        public SolutionDTO Solution { get; set; }
        public Guid OwnerId { get; set; }
    }
}

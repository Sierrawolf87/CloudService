﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Models
{
    public class Solution
    {
        [Key]
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public int Mark { get; set; }

        public Guid LaboratoryWorkId { get; set; }
        [ForeignKey("LaboratoryWorkId")]
        public LaboratoryWork LaboratoryWork { get; set; }

        public List<File> Files { get; set; }

        public Solution() {}
        public Solution(string description, Guid ownerId, int mark, Guid laboratoryWorkId, List<File> files)
        {
            Description = description;
            OwnerId = ownerId;
            Mark = mark;
            LaboratoryWorkId = laboratoryWorkId;
            Files.AddRange(files);
        }

        public Solution(string description, Guid ownerId, int mark, Guid laboratoryWorkId)
        {
            Description = description;
            OwnerId = ownerId;
            Mark = mark;
            LaboratoryWorkId = laboratoryWorkId;
            Files = null;
        }

        public SolutionDTO ToSolutionDto()
        {
            return new SolutionDTO(Id, OwnerId, Description, Mark, LaboratoryWorkId, Files);
        }
    }

    public class SolutionDTO
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Description { get; set; }
        public int Mark { get; set; }
        public Guid LaboratoryWorkId { get; set; }

        public List<FileDTO> Files { get; set; }

        public SolutionDTO() {}
        public SolutionDTO(Guid id, Guid ownerId, string description, int mark, Guid laboratoryWorkId, List<File> files)
        {
            Id = id;
            OwnerId = ownerId;
            Description = description;
            Mark = mark;
            LaboratoryWorkId = laboratoryWorkId;
            if (files != null)
            {
                Files = new List<FileDTO>();
                foreach (var item in files)
                {
                    Files.Add(item.ToFileDto());
                }
            }
            else
            {
                Files = new List<FileDTO>();
            }
        }
    }

    public class CreateSolutionDTO
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Mark { get; set; }
        public Guid LaboratoryWorkId { get; set; }
    }
}

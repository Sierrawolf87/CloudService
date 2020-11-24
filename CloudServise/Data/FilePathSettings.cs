using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Data
{
    public class FilePathSettings : IValidatableObject
    {
        public string FolderForFiles { get; set; }

        public FilePathSettings() {}
        public FilePathSettings(FilePathSettings settings)
        {
            FolderForFiles = settings.FolderForFiles;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();
            if (string.IsNullOrWhiteSpace(FolderForFiles))
            {
                FolderForFiles = "C:\\CloudService\\Files";
                errors.Add(new ValidationResult(
                    $"Для параметра {GetType().Name} -> FolderForFiles задано значение по умолчанию '{FolderForFiles}'"));
            }

            return errors;
        }
    }
}

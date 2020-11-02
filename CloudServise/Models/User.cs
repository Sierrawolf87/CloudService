using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudServise_API.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string ReportCard { get; set; }

        public string Role { get; set; }

        public List<DisciplineGroupTeacher> DisciplineGroupTeachers { get; set; }


        public User(string email, string userName, string name, string surname, string patronymic, string reportCard)
        {
            Id = Guid.NewGuid();
            Email = email;
            UserName = userName;
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            ReportCard = reportCard;
        }

        public User(string name, string surname, string patronymic, string reportCard)
        {
            Id = Guid.NewGuid();
            UserName = reportCard;
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            ReportCard = reportCard;
        }
    }
}

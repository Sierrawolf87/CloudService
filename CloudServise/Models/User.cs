using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CloudServise_API.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CloudServise_API.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string ReportCard { get; set; }

        public Role Role { get; set; }

        public List<DisciplineGroupTeacher> DisciplineGroupTeachers { get; set; }
        
        public User() {}
        public User(string email, string userName, string password, string name, string surname, string patronymic, string reportCard, Role role)
        {
            Email = email;
            UserName = userName;
            Password = Auxiliary.GenerateHashPassword(password);
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            ReportCard = reportCard;
            Role = role;
        }

        public User(string name, string surname, string patronymic, string reportCard, Role role)
        {
            UserName = reportCard;
            Password = Auxiliary.GenerateHashPassword(reportCard);
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            ReportCard = reportCard;
            Role = role;
        }

        public UserDTO ToUserDto()
        {
            UserDTO userDto = new UserDTO();
            userDto.Id = Id;
            userDto.Email = Email;
            userDto.Name = Name;
            userDto.Surname = Surname;
            userDto.Patronymic = Patronymic;
            userDto.ReportCard = ReportCard;
            userDto.Role = Role.ToRoleDTO();
            return userDto;
        }


    }

    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Initials => !string.IsNullOrEmpty(Patronymic) ? $"{Surname} {Name[0]}. {Patronymic[0]}." : $"{Surname} {Name[0]}";
        public string ReportCard { get; set; }

        public RoleDTO Role { get; set; }

    }
}

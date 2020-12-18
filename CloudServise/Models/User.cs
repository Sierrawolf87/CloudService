using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CloudService_API.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CloudService_API.Models
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
        public Group Group { get; set; }

        public List<DisciplineGroupTeacher> DisciplineGroupTeachers { get; set; }

        public User() {}
        public User(string email, string userName, string password, string name, string surname, string patronymic, string reportCard, Role role, Group group, string hashKey)
        {
            Email = email;
            UserName = userName;
            Password = Auxiliary.GenerateHashPassword(password, hashKey);
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            ReportCard = reportCard;
            Role = role;
            Group = group;
        }

        public User(string name, string surname, string patronymic, string reportCard, Role role, Group group, string hashKey)
        {
            UserName = reportCard;
            Password = Auxiliary.GenerateHashPassword(reportCard, hashKey);
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            ReportCard = reportCard;
            Role = role;
            Group = group;
        }

        public UserDTO ToUserDto()
        {
            UserDTO userDto = new UserDTO();
            userDto.Id = Id;
            userDto.UserName = UserName;
            userDto.Email = Email;
            userDto.Name = Name;
            userDto.Surname = Surname;
            userDto.Patronymic = Patronymic;
            userDto.ReportCard = ReportCard;
            userDto.Role = Role?.ToRoleDTO();
            userDto.Group = Group?.ToGroupDto();
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
        public GroupDTO Group { get; set; }

    }

    public class UserRegisterDTO
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string ReportCard { get; set; }

        public Guid RoleId { get; set; }

        public Guid GroupId { get; set; }
    }

    public class ForgotPassword
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public ForgotPassword() {}
        public ForgotPassword(Guid id, DateTime dateTime)
        {
            Id = id;
            DateTime = dateTime;
        }
    }

    public class ResetPassword
    {
        public string NewPassword { get; set; }
        public string ConfimPassword { get; set; }
    }

    public class ResetPasswordSelf
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfimPassword { get; set; }
    }

    public class ChangeEmail
    {
        [EmailAddress(ErrorMessage = "Неверная запись почты")]
        public string NewEmail { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudService_API.Models;

namespace CloudService_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<DisciplineGroupTeacher> DisciplineGroupTeacher { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<LaboratoryWork> LaboratoryWorks { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<Solution> Solutions { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<User>().HasData(new List<User>
        //    {
        //        new User
        //        {
        //            Id = new Guid("B5933B9F-69D6-41D3-7456-08D89C4C1685"),
        //            Email = null,
        //            UserName = "root",
        //            Name = "root",
        //            Surname = "root",
        //            Patronymic = "root",
        //            Password = "ZWN9JfZx0sM/ajRBk/0+qwnMpjfJuWwXfkLiGglT4YA=", //root
        //        }
        //    });
        //}
    }
}

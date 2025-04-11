using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using ToDoList.Models;

namespace ToDoList.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<GlobalCategory> GlobalCategories { get; set; }
        public DbSet<UserCategory> UserCategories { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskCategory>()
                .HasKey(tc => tc.Id);

            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.TaskCategories)
                .HasForeignKey(tc => tc.TaskId);

            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.GlobalCategory)
                .WithMany(gc => gc.TaskCategories)
                .HasForeignKey(tc => tc.GlobalCategoryId)
                .IsRequired(false);

            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.UserCategory)
                .WithMany(uc => uc.TaskCategories)
                .HasForeignKey(tc => tc.UserCategoryId)
                .IsRequired(false);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .IsRequired(true);

            modelBuilder.Entity<UserCategory>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCategories)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired(true);

            modelBuilder.Entity<GlobalCategory>()
                .HasIndex(gc => gc.Name)
                .IsUnique();

            modelBuilder.Entity<UserCategory>()
                .HasIndex(uc => new { uc.Name, uc.UserId })
                .IsUnique();

            modelBuilder.Entity<GlobalCategory>().HasData(
                new GlobalCategory { Id = 1, Name = "Работа" },
                new GlobalCategory { Id = 2, Name = "Учеба" },
                new GlobalCategory { Id = 3, Name = "Дом и быт" }
            );
        }
    }
}
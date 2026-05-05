using LMSDataExtraction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMSDataExtraction.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Activity> Activities { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Users = Set<User>();
        Courses = Set<Course>();
        Modules = Set<Module>();
        Assignments = Set<Assignment>();
        Activities = Set<Activity>();
    }
}

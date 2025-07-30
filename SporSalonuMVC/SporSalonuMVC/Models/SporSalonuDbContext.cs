using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

public class SporSalonuDbContext : DbContext
{
    public SporSalonuDbContext(DbContextOptions<SporSalonuDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<UserMembership> UserMemberships { get; set; }
    public DbSet<StudentDocument> StudentDocuments { get; set; }
    public DbSet<UserCard> UserCards { get; set; }
    public DbSet<EntryLog> EntryLogs { get; set; }


}

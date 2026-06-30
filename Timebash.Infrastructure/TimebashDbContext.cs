using Microsoft.EntityFrameworkCore;
using Timebash.Core.Contracts;

namespace Timebash.Infrastructure;

public class TimebashDbContext(DbContextOptions<TimebashDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Activity> Activities { get; private set; }
    public DbSet<Category> Categories { get; private set; }
    public DbSet<Journal> Journals { get; private set; }
    public DbSet<User> Users { get; private set; }
    public DbSet<UserSettings> UserSettings { get; private set; }

    public DbSet<ActivityCategory> ActivityCategories { get; private set; }

    public Task SaveChangesAsync() => base.SaveChangesAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasIndex(a => new { a.JournalId, a.StartTime, a.EndTime });
            entity.HasIndex(a => new { a.JournalId, a.EndTime, a.StartTime })
                .IsDescending(false, true, true);
        });

        modelBuilder.Entity<ActivityCategory>(entity =>
        {
            entity.HasKey(pair => new { pair.ActivityId, pair.CategoryId });
            entity.HasIndex(ac => ac.CategoryId);
        });
            
        modelBuilder.Entity<Category>()
            .Property(category => category.Keywords)
            .HasColumnType("jsonb");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Name).IsUnique();
        });

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(settings => settings.UserId);

            entity.HasOne(settings => settings.User)
                .WithOne()
                .HasForeignKey<UserSettings>(settings => settings.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(settings => settings.DefaultJournal)
                .WithMany()
                .HasForeignKey(settings => settings.DefaultJournalId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

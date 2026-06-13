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

        modelBuilder.Entity<ActivityCategory>()
            .HasKey(pair => new { pair.ActivityId, pair.CategoryId });

        modelBuilder.Entity<Category>()
            .Property(category => category.Keywords)
            .HasColumnType("jsonb");
        
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

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace VlasikhaPlavanieWebsite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<StatItem> StatItems { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var valueComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),
                c => c.ToList());

            builder.Entity<StatItem>(b =>
            {
                b.Property(e => e.Files)
                    .HasConversion(
                        v => string.Join(';', v),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .Metadata.SetValueComparer(valueComparer);
            });

            // Указываем первичные ключи и связи
            builder.Entity<Participant>()
                .HasKey(p => p.Id);

            builder.Entity<Participant>()
                .HasMany(p => p.Disciplines)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Discipline>()
                .HasKey(d => d.Id);

            builder.Entity<Discipline>()
                .Property(d => d.Name)
                .IsRequired();

            builder.Entity<Discipline>()
                .Property(d => d.Distance)
                .IsRequired();
        }
    }
}

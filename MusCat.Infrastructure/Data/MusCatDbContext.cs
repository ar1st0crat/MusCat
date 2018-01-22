using System.Data.Entity;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Data
{
    public class MusCatDbContext : DbContext
    {
        public MusCatDbContext() : base("name=MusCatDbContext")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<MusCatDbContext>());
        }

        public virtual DbSet<Album> Albums { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Genre> Genres { get; set; }
        public virtual DbSet<Lineup> Lineups { get; set; }
        public virtual DbSet<Musician> Musicians { get; set; }
        public virtual DbSet<Performer> Performers { get; set; }
        public virtual DbSet<Song> Songs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Album>()
                .Property(e => e.TotalTime)
                .IsUnicode(false);

            modelBuilder.Entity<Album>()
                .Property(e => e.Info)
                .IsUnicode(false);

            modelBuilder.Entity<Country>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Genre>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Genre>()
                .HasMany(e => e.Performers)
                .WithMany(e => e.Genres)
                .Map(m => m.ToTable("PerformerGenres").MapLeftKey("GenreID").MapRightKey("PerformerID"));

            modelBuilder.Entity<Musician>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Musician>()
                .Property(e => e.Info)
                .IsUnicode(false);

            modelBuilder.Entity<Performer>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Performer>()
                .Property(e => e.Info)
                .IsUnicode(false);

            modelBuilder.Entity<Song>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Song>()
                .Property(e => e.TimeLength)
                .IsUnicode(false);
        }
    }
}

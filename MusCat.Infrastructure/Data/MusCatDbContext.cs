using System.ComponentModel.DataAnnotations.Schema;
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
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Album>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            modelBuilder.Entity<Album>()
                .Property(e => e.TotalTime)
                .HasMaxLength(6)
                .IsUnicode(false);

            modelBuilder.Entity<Album>()
                .Property(e => e.Info)
                .HasColumnType("text")
                .IsUnicode(false);

            modelBuilder.Entity<Country>()
                .Property(e => e.Name)
                .HasMaxLength(20)
                .IsRequired()
                .IsUnicode(false);

            modelBuilder.Entity<Genre>()
                .Property(e => e.Name)
                .HasMaxLength(25)
                .IsRequired()
                .IsUnicode(false);

            modelBuilder.Entity<Genre>()
                .HasMany(e => e.Performers)
                .WithMany(e => e.Genres)
                .Map(m => m.ToTable("PerformerGenres").MapLeftKey("GenreID").MapRightKey("PerformerID"));

            modelBuilder.Entity<Lineup>()
                .HasKey(k => new { k.PerformerID, k.MusicianID });

            modelBuilder.Entity<Lineup>()
                .Property(e => e.PerformerID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(0);

            modelBuilder.Entity<Lineup>()
                .Property(e => e.MusicianID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(1);

            modelBuilder.Entity<Musician>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false);

            modelBuilder.Entity<Musician>()
                .Property(e => e.Info)
                .HasColumnType("text")
                .IsUnicode(false);

            modelBuilder.Entity<Performer>()
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Performer>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);

            modelBuilder.Entity<Performer>()
                .Property(e => e.Info)
                .HasColumnType("text")
                .IsUnicode(false);

            modelBuilder.Entity<Song>()
               .Property(e => e.ID)
               .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Song>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            modelBuilder.Entity<Song>()
                .Property(e => e.TimeLength)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false);
        }
    }
}

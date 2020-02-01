using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using MusCat.Application.Validators;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Data
{
    public class MusCatDbContext : DbContext
    {
        public MusCatDbContext(string conn) : base(conn)
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
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Album>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(AlbumNameMaxLength)
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
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Country>()
                .Property(e => e.Name)
                .HasMaxLength(CountryNameMaxLength)
                .IsRequired()
                .IsUnicode(false);

            modelBuilder.Entity<Genre>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Genre>()
                .Property(e => e.Name)
                .HasMaxLength(25)
                .IsRequired()
                .IsUnicode(false);

            modelBuilder.Entity<Genre>()
                .HasMany(e => e.Performers)
                .WithMany(e => e.Genres)
                .Map(m => m.ToTable("PerformerGenres").MapLeftKey("GenreId").MapRightKey("PerformerId"));

            modelBuilder.Entity<Lineup>()
                .HasKey(k => new { k.PerformerId, k.MusicianId });

            modelBuilder.Entity<Lineup>()
                .Property(e => e.PerformerId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(0);

            modelBuilder.Entity<Lineup>()
                .Property(e => e.MusicianId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
                .HasColumnOrder(1);

            modelBuilder.Entity<Musician>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

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
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Performer>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(PerformerNameMaxLength)
                .IsUnicode(false);

            modelBuilder.Entity<Performer>()
                .Property(e => e.Info)
                .HasColumnType("text")
                .IsUnicode(false);

            modelBuilder.Entity<Song>()
               .Property(e => e.Id)
               .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Song>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(SongNameMaxLength)
                .IsUnicode(false);

            modelBuilder.Entity<Song>()
                .Property(e => e.TimeLength)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false);


            // quite non-conventional approach
            // (these constants are used in VM validation code too):

            PerformerValidator.PerformerNameMaxLength = PerformerNameMaxLength;
            AlbumValidator.AlbumNameMaxLength = AlbumNameMaxLength;
            SongValidator.SongNameMaxLength = SongNameMaxLength;
            CountryValidator.CountryNameMaxLength = CountryNameMaxLength;
        }

        // quite non-conventional approach
        // (these constants are used in VM validation code too):

        public const int PerformerNameMaxLength = 30;
        public const int AlbumNameMaxLength = 50;
        public const int SongNameMaxLength = 50;
        public const int CountryNameMaxLength = 20;
    }
}

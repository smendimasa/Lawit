using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LawIT.Models.LawITContextModels
{
    public partial class LawITContext : DbContext
    {
        public LawITContext()
        {
        }

        public LawITContext(DbContextOptions<LawITContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Document> Document { get; set; }
        public virtual DbSet<DocumentWord> DocumentWord { get; set; }
        public virtual DbSet<Subtitle> Subtitle { get; set; }
        public virtual DbSet<Title> Title { get; set; }
        public virtual DbSet<Word> Word { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=lawit.database.windows.net;Database=LawIT;User ID=lawitbot;Password=Lawitdbpa55");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(e => e.DocumentHeader)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.UniversalCitation)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Title)
                    .WithMany(p => p.Document)
                    .HasForeignKey(d => d.TitleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Document_Title");
            });

            modelBuilder.Entity<DocumentWord>(entity =>
            {
                entity.HasOne(d => d.Document)
                    .WithMany(p => p.DocumentWord)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocumentWord_Document");

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.DocumentWord)
                    .HasForeignKey(d => d.WordId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocumentWord_Word");
            });

            modelBuilder.Entity<Subtitle>(entity =>
            {
                entity.Property(e => e.SubtitleName)
                    .HasMaxLength(600)
                    .IsUnicode(false);

                entity.Property(e => e.SubtitleNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Title)
                    .WithMany(p => p.Subtitle)
                    .HasForeignKey(d => d.TitleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subtitle_Title");
            });

            modelBuilder.Entity<Title>(entity =>
            {
                entity.Property(e => e.TitleName)
                    .HasMaxLength(600)
                    .IsUnicode(false);

                entity.Property(e => e.TitleNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Word>(entity =>
            {
                entity.Property(e => e.Word1)
                    .IsRequired()
                    .HasColumnName("Word")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
        }
    }
}

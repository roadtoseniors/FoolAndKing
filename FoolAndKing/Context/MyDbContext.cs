using System;
using System.Collections.Generic;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Claim> Claims { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Genrebook> Genrebooks { get; set; }

    public virtual DbSet<Readinglist> Readinglists { get; set; }

    public virtual DbSet<Reason> Reasons { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Requestfrozen> Requestfrozens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userreadinglist> Userreadinglists { get; set; }

    public virtual DbSet<VwBookInfo> VwBookInfos { get; set; }

    public virtual DbSet<VwUserInfo> VwUserInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseLazyLoadingProxies().UseSqlServer("Data Source=DESKTOP-AB4C90S;Initial Catalog=FoolAndKingDB;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("book");

            entity.HasIndex(e => e.AuthorId, "IX_book_AuthorID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.CoverPath).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_book_author");
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.ToTable("claim");

            entity.HasIndex(e => e.BookId, "IX_claim_BookID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FeedBackId).HasColumnName("FeedBackID");
            entity.Property(e => e.ReasonId).HasColumnName("ReasonID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Book).WithMany(p => p.Claims)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_claim_book");

            entity.HasOne(d => d.FeedBack).WithMany(p => p.Claims)
                .HasForeignKey(d => d.FeedBackId)
                .HasConstraintName("FK_claim_feedback");

            entity.HasOne(d => d.Reason).WithMany(p => p.Claims)
                .HasForeignKey(d => d.ReasonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_reason");

            entity.HasOne(d => d.User).WithMany(p => p.Claims)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claim_user");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("feedback");

            entity.HasIndex(e => e.BookId, "IX_feedback_BookID");

            entity.HasIndex(e => e.UserId, "IX_feedback_UserID");

            entity.HasIndex(e => new { e.UserId, e.BookId }, "UQ_feedback_UserBook").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Book).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_feedback_book");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_feedback_user");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("genre");

            entity.HasIndex(e => e.Name, "UQ_genre_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Genrebook>(entity =>
        {
            entity.ToTable("genrebook");

            entity.HasIndex(e => new { e.GenreId, e.BookId }, "UQ_genrebook").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.GenreId).HasColumnName("GenreID");

            entity.HasOne(d => d.Book).WithMany(p => p.Genrebooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_genrebook_book");

            entity.HasOne(d => d.Genre).WithMany(p => p.Genrebooks)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_genrebook_genre");
        });

        modelBuilder.Entity<Readinglist>(entity =>
        {
            entity.ToTable("readinglist");

            entity.HasIndex(e => e.Name, "UQ_readinglist_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Reason>(entity =>
        {
            entity.ToTable("reason");

            entity.HasIndex(e => e.Name, "UQ_reason_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("request");

            entity.HasIndex(e => e.UserId, "IX_request_UserID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Role).WithMany(p => p.Requests)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_request_role");

            entity.HasOne(d => d.User).WithMany(p => p.Requests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_request_user");
        });

        modelBuilder.Entity<Requestfrozen>(entity =>
        {
            entity.ToTable("requestfrozen");

            entity.HasIndex(e => e.UserId, "IX_requestfrozen_UserID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Requestfrozens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_requestfrozen_user");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "UQ_role_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ_user_Email").IsUnique();

            entity.HasIndex(e => e.Login, "UQ_user_Login").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_role");
        });

        modelBuilder.Entity<Userreadinglist>(entity =>
        {
            entity.ToTable("userreadinglist");

            entity.HasIndex(e => e.UserId, "IX_url_UserID");

            entity.HasIndex(e => new { e.UserId, e.BookId }, "UQ_userreadinglist").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.ReadingListId).HasColumnName("ReadingListID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Book).WithMany(p => p.Userreadinglists)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userreadinglist_book");

            entity.HasOne(d => d.ReadingList).WithMany(p => p.Userreadinglists)
                .HasForeignKey(d => d.ReadingListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userreadinglist_status");

            entity.HasOne(d => d.User).WithMany(p => p.Userreadinglists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userreadinglist_user");
        });

        modelBuilder.Entity<VwBookInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_BookInfo");

            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.AuthorName).HasMaxLength(100);
            entity.Property(e => e.AvgScore).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.BookName).HasMaxLength(200);
            entity.Property(e => e.CoverPath).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<VwUserInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserInfo");

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

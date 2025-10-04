using System;
using System.Collections.Generic;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace FitnessPT_api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Routine> Routines { get; set; }

    public virtual DbSet<RoutineExercise> RoutineExercises { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=redhorse.iptime.org;port=3306;database=FitnessPT_api;user=gun;password=gks7646!^^", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("exercises");

            entity.HasIndex(e => e.Category, "idx_category");

            entity.HasIndex(e => e.Level, "idx_level");

            entity.HasIndex(e => new { e.Level, e.Category }, "idx_level_category");

            entity.HasIndex(e => e.Name, "idx_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasColumnType("enum('upper_body','lower_body','cardio','core','full_body')")
                .HasColumnName("category");
            entity.Property(e => e.CategoryDetail)
                .HasMaxLength(50)
                .HasColumnName("category_detail");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Level)
                .HasColumnType("enum('beginner','intermediate','advanced')")
                .HasColumnName("level");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(255)
                .HasColumnName("video_url");
        });

        modelBuilder.Entity<Routine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("routines");

            entity.HasIndex(e => e.Category, "idx_category");

            entity.HasIndex(e => e.CreatedUser, "idx_created_user");

            entity.HasIndex(e => e.Level, "idx_level");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasColumnType("enum('upper_body','lower_body','cardio','core','full_body')")
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedUser)
                .HasComment("NULL이면 관리자, 값이 있으면 사용자")
                .HasColumnName("created_user");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EstimatedDuration)
                .HasComment("예상 소요 시간(분)")
                .HasColumnName("estimated_duration");
            entity.Property(e => e.Level)
                .HasColumnType("enum('beginner','intermediate','advanced')")
                .HasColumnName("level");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(255)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.Routines)
                .HasForeignKey(d => d.CreatedUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("routines_ibfk_1");
        });

        modelBuilder.Entity<RoutineExercise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("routine_exercises");

            entity.HasIndex(e => e.ExerciseId, "idx_exercise_id");

            entity.HasIndex(e => e.RoutineId, "idx_routine_id");

            entity.HasIndex(e => new { e.RoutineId, e.OrderIndex }, "unique_routine_order").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationSeconds)
                .HasComment("시간 기반 운동(초)")
                .HasColumnName("duration_seconds");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.OrderIndex)
                .HasComment("루틴 내 운동 순서")
                .HasColumnName("order_index");
            entity.Property(e => e.Reps)
                .HasComment("반복 횟수")
                .HasColumnName("reps");
            entity.Property(e => e.RestSeconds)
                .HasDefaultValueSql("'60'")
                .HasComment("세트 간 휴식(초)")
                .HasColumnName("rest_seconds");
            entity.Property(e => e.RoutineId).HasColumnName("routine_id");
            entity.Property(e => e.Sets)
                .HasDefaultValueSql("'3'")
                .HasComment("세트 수")
                .HasColumnName("sets");

            entity.HasOne(d => d.Exercise).WithMany(p => p.RoutineExercises)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("routine_exercises_ibfk_2");

            entity.HasOne(d => d.Routine).WithMany(p => p.RoutineExercises)
                .HasForeignKey(d => d.RoutineId)
                .HasConstraintName("routine_exercises_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "idx_email").IsUnique();

            entity.HasIndex(e => e.GoogleId, "idx_google_id").IsUnique();

            entity.HasIndex(e => e.Role, "idx_role");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.GoogleId).HasColumnName("google_id");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt)
                .HasMaxLength(6)
                .HasColumnName("last_login_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(500)
                .HasColumnName("profile_image_url");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'USER'")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

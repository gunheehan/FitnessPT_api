using System;
using System.Collections.Generic;
using FitnessPT_api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessPT_api.Data;

public partial class FitnessDbContext : DbContext
{
    public FitnessDbContext()
    {
    }

    public FitnessDbContext(DbContextOptions<FitnessDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bodyrecord> Bodyrecords { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Exercisecategory> Exercisecategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userprofile> Userprofiles { get; set; }

    public virtual DbSet<Workoutrecord> Workoutrecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=redhorse.iptime.org;Database=fitnesspt;Username=fitnessadmin;Password=gks7646!^^");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bodyrecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("bodyrecords_pkey");

            entity.ToTable("bodyrecords");

            entity.HasIndex(e => new { e.UserId, e.RecordedDate }, "idx_body_records_user_date");

            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.BodyFatPercentage)
                .HasPrecision(4, 2)
                .HasColumnName("body_fat_percentage");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.MuscleMassKg)
                .HasPrecision(5, 2)
                .HasColumnName("muscle_mass_kg");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RecordedDate).HasColumnName("recorded_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeightKg)
                .HasPrecision(5, 2)
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.User).WithMany(p => p.Bodyrecords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("bodyrecords_user_id_fkey");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("exercises_pkey");

            entity.ToTable("exercises");

            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DifficultyLevel).HasColumnName("difficulty_level");
            entity.Property(e => e.ExerciseName)
                .HasMaxLength(100)
                .HasColumnName("exercise_name");
            entity.Property(e => e.Instructions).HasColumnName("instructions");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PrimaryCategoryId).HasColumnName("primary_category_id");
            entity.Property(e => e.TargetMuscles).HasColumnName("target_muscles");

            entity.HasOne(d => d.PrimaryCategory).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.PrimaryCategoryId)
                .HasConstraintName("exercises_primary_category_id_fkey");
        });

        modelBuilder.Entity<Exercisecategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("exercisecategories_pkey");

            entity.ToTable("exercisecategories");

            entity.HasIndex(e => e.CategoryCode, "exercisecategories_category_code_key").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .HasColumnName("category_code");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("exercisecategories_parent_category_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.GoogleId, "idx_users_google_id");

            entity.HasIndex(e => e.Username, "idx_users_username");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.GoogleId, "users_google_id_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(50)
                .HasColumnName("google_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Role)
                .HasDefaultValue((short)1)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(30)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Userprofile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("userprofiles_pkey");

            entity.ToTable("userprofiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentWeightKg)
                .HasPrecision(5, 2)
                .HasColumnName("current_weight_kg");
            entity.Property(e => e.FitnessGoal)
                .HasMaxLength(50)
                .HasColumnName("fitness_goal");
            entity.Property(e => e.FitnessLevel)
                .HasMaxLength(20)
                .HasColumnName("fitness_level");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.HeightCm)
                .HasPrecision(5, 2)
                .HasColumnName("height_cm");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User).WithOne(p => p.Userprofile)
                .HasForeignKey<Userprofile>(d => d.UserId)
                .HasConstraintName("userprofiles_user_id_fkey");
        });

        modelBuilder.Entity<Workoutrecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("workoutrecords_pkey");

            entity.ToTable("workoutrecords");

            entity.HasIndex(e => e.ExerciseId, "idx_workout_records_exercise");

            entity.HasIndex(e => new { e.UserId, e.WorkoutDate }, "idx_workout_records_user_date");

            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SetsData)
                .HasColumnType("jsonb")
                .HasColumnName("sets_data");
            entity.Property(e => e.TotalDurationMinutes).HasColumnName("total_duration_minutes");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkoutDate).HasColumnName("workout_date");

            entity.HasOne(d => d.Exercise).WithMany(p => p.Workoutrecords)
                .HasForeignKey(d => d.ExerciseId)
                .HasConstraintName("workoutrecords_exercise_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Workoutrecords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("workoutrecords_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

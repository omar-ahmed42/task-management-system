using backend.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = backend.Entities.Task;

namespace backend.Data;

public class TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Role>(b =>
        {
            b.ToTable("roles");
            b.HasKey(r => r.Id).HasName("role_pk");
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.NormalizedName).HasColumnName("normalized_name");
            b.Property(r => r.Name).HasColumnName("name");
            b.Property(r => r.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        });

        builder.Entity<User>(b =>
 {
     // Primary key
     b.HasKey(u => u.Id).HasName("user_pk");
     b.Property(u => u.Id).HasColumnName("id");

     // Indexes for "normalized" username and email, to allow efficient lookups
     b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("username_index").IsUnique();
     b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("email_index");

     // Maps to the AspNetUsers table
     b.ToTable("users");

     // A concurrency token for use with the optimistic concurrency checking
     b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken().HasColumnName("concurrency_stamp");

     // Limit the size of columns to use efficient database types
     b.Property(u => u.UserName).HasMaxLength(256).HasColumnName("username");
     b.Property(u => u.NormalizedUserName).HasMaxLength(256).HasColumnName("normalized_username");
     b.Property(u => u.Email).HasMaxLength(256).HasColumnName("email");
     b.Property(u => u.NormalizedEmail).HasMaxLength(256).HasColumnName("normalized_email");

     b.Property(u => u.PasswordHash).HasColumnName("password");
     b.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");
     b.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
     b.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
     b.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");

     b.Ignore(u => u.PhoneNumber)
        .Ignore(u => u.AccessFailedCount)
        .Ignore(u => u.PhoneNumber)
        .Ignore(u => u.PhoneNumberConfirmed)
        .Ignore(u => u.LockoutEnabled)
        .Ignore(u => u.LockoutEnd);
 });
        builder.Entity<Role>().HasData(
            new {Id = Guid.Parse("cd8f322d-44b0-443f-b769-4cb303fdb6c0"), Name = "admin", Description = "This is a role that represents superusers/admins.", NormalizedName = "ADMIN", ConcurrencyStamp = "b5421663-270f-478f-ac62-cc0bddd6985f"},
            new {Id = Guid.Parse("840b142e-a5ca-4607-a67e-875c1c068c75"), Name = "user", Description = "This is a role that represents regular users and team leaders.", NormalizedName = "USER", ConcurrencyStamp = "849db589-38e2-4a95-bb8c-5558b6eee8be"}
        );
    }

}

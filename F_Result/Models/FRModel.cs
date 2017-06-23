namespace F_Result.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Migrations;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.ComponentModel.DataAnnotations;

    public sealed class Configuration : DbMigrationsConfiguration<FRModel>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "F_Result.Models.FRModel";
        }

        protected override void Seed(FRModel db)
        {

        }
    }

    class FRContextInitializer : MigrateDatabaseToLatestVersion<FRModel, Configuration>
    {

    }

    public partial class FRModel : DbContext
    {
        static FRModel()
        {
            Database.SetInitializer<FRModel>(new FRContextInitializer());
        }

        public FRModel()
            : base("name=FResult")
        {
        }

        public DbSet<F_Result.Models.Payments> Payments { get; set; }
        public DbSet<F_Result.Models.Projects> Projects { get; set; }
        public DbSet<F_Result.Models.Organizations> Organizations { get; set; }
        public DbSet<F_Result.Models.DUsers> DUsers { get; set; }
        public DbSet<F_Result.Models.ActualDebit> ActualDebit { get; set; }
        public DbSet<AspVisitor> AspVisitors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payments>()
                .HasKey(t => t.id)
                .ToTable("DirectumPayments");

            modelBuilder.Entity<Projects>()
                .HasKey(t => t.id)
                .ToTable("Projects");

            modelBuilder.Entity<DUsers>()
                .HasKey(t => t.id)
                .ToTable("Users");

            modelBuilder.Entity<Organizations>()
                .HasKey(t => t.id)
                .ToTable("Organizations");

            modelBuilder.Entity<Payments>().Property(p => p.Summ)
                 .HasPrecision(10, 2);
            modelBuilder.Entity<Payments>().Property(p => p.Payment)
                 .HasPrecision(10, 2);
            modelBuilder.Entity<ActualDebit>().Property(p => p.Sum)
                 .HasPrecision(10, 2);
            modelBuilder.Entity<PlanCredit>().Property(p => p.Sum)
                .HasPrecision(10, 2);

            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id).ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId).ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId }).ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("AspNetUserClaims");

            modelBuilder.Entity<ActualDebit>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.ActualDebit)
            .HasForeignKey(m => m.UserId);

            modelBuilder.Entity<PlanCredit>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.PlanCredit)
            .HasForeignKey(m => m.UserId);
        }

        public System.Data.Entity.DbSet<F_Result.Models.ApplicationUser> IdentityUsers { get; set; }

        public System.Data.Entity.DbSet<F_Result.Models.PlanCredit> PlanCredits { get; set; }
    }
}

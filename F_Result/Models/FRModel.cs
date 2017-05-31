namespace F_Result.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.ModelConfiguration;


    public partial class FRModel : DbContext
    {
        public FRModel()
            : base("name=FResult")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payments>()
                .HasKey(t => t.id)
                .ToTable("DirectumPayments");

            modelBuilder.Entity<Projects>()
                .HasKey(t => t.id)
                .ToTable("Projects");

            modelBuilder.Entity<DUsers>()
                .HasKey(t => t.id)
                .ToTable("Users");

            modelBuilder.Ignore<Payments>();
            modelBuilder.Ignore<Projects>();
            modelBuilder.Ignore<DUsers>();
            modelBuilder.Ignore<Organizations>();
        }

        public DbSet<F_Result.Models.Payments> Payments { get; set; }
        public DbSet<F_Result.Models.Projects> Projects { get; set; }
        public DbSet<F_Result.Models.Organizations> Organizations { get; set; }
        public DbSet<F_Result.Models.DUsers> DUsers { get; set; }
        public DbSet<F_Result.Models.ActualDebit> ActualDebit { get; set; }
    }
}

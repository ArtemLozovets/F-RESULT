namespace F_Result.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.ModelConfiguration;


    public class DirectumPaymentsConfiguration : EntityTypeConfiguration<Payments>
    {
        public DirectumPaymentsConfiguration()
        {
            this.HasKey(t => t.id);
            this.ToTable("DirectumPayments");
        }
    }

    public class DirectumProjectsConfiguration : EntityTypeConfiguration<Projects>
    {
        public DirectumProjectsConfiguration()
        {
            this.HasKey(t => t.id);
            this.ToTable("Projects");
        }
    }

    public class DirectumUsersConfiguration : EntityTypeConfiguration<DUsers>
    {
        public DirectumUsersConfiguration()
        {
            this.HasKey(t => t.id);
            this.ToTable("Users");
        }
    }

    public partial class FRModel : DbContext
    {
        public FRModel()
            : base("name=FResult")
        {
        }

        public virtual DbSet<Person> Persons { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .Property(e => e.fname)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.minit)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.lname)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.pub_id)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Configurations.Add(new DirectumPaymentsConfiguration());
            modelBuilder.Configurations.Add(new DirectumProjectsConfiguration());
            modelBuilder.Configurations.Add(new DirectumUsersConfiguration());
        }

        public System.Data.Entity.DbSet<F_Result.Models.Payments> Payments { get; set; }
        public System.Data.Entity.DbSet<F_Result.Models.Projects> Projects { get; set; }
        public System.Data.Entity.DbSet<F_Result.Models.Organizations> Organizations { get; set; }
        public System.Data.Entity.DbSet<F_Result.Models.DUsers> DUsers { get; set; }
    }
}

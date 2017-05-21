namespace F_Result.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.ModelConfiguration;


    public class DirectumViewConfiguration : EntityTypeConfiguration<Payments>
    {
        public DirectumViewConfiguration()
        {
            this.HasKey(t => t.AgrDate);
            this.ToTable("DirectumPayments");
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

            modelBuilder.Configurations.Add(new DirectumViewConfiguration());
        }

        public System.Data.Entity.DbSet<F_Result.Models.Payments> Payments { get; set; }
    }
}

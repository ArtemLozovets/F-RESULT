namespace F_Result.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FRModel : DbContext
    {
        public FRModel()
            : base("name=FResult")
        {
        }

        public virtual DbSet<employee> employees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<employee>()
                .Property(e => e.fname)
                .IsUnicode(false);

            modelBuilder.Entity<employee>()
                .Property(e => e.minit)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<employee>()
                .Property(e => e.lname)
                .IsUnicode(false);

            modelBuilder.Entity<employee>()
                .Property(e => e.pub_id)
                .IsFixedLength()
                .IsUnicode(false);
        }
    }
}

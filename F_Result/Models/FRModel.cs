namespace F_Result.Models
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using Microsoft.AspNet.Identity.EntityFramework;

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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payments>()
                .HasKey(t => t.id)
                .ToTable("DirectumPayments");

            modelBuilder.Entity<PaymentsF2>()
                .HasKey(t => t.id)
                .ToTable("DirectumPaymentsF2");

            modelBuilder.Entity<ActualDebitsF2>()
                .HasKey(t => t.id)
                .ToTable("ActualDebitsF2");

            modelBuilder.Entity<ActualDebitsF1>()
               .HasKey(t => t.id)
               .ToTable("ActualDebitsF1");

            modelBuilder.Entity<Projects>()
                .HasKey(t => t.id)
                .ToTable("Projects");

            modelBuilder.Entity<DUsers>()
                .HasKey(t => t.id)
                .ToTable("Users");

            modelBuilder.Entity<Workers>()
                .HasKey(t => t.id)
                .ToTable("Workers");

            modelBuilder.Entity<Expenditure>()
                .HasKey(t => t.Id)
                .ToTable("Expenditures");

            modelBuilder.Entity<Income>()
                .HasKey(t => t.Id)
                .ToTable("Incomes");

            modelBuilder.Entity<Organizations>()
                .HasKey(t => t.id)
                .ToTable("Organizations");

            modelBuilder.Entity<AAOReport>()
                .HasKey(t => t.ID)
                .ToTable("AaoReport");

            modelBuilder.Entity<ActualDebit>().Property(p => p.Sum)
                 .HasPrecision(12, 2);
            modelBuilder.Entity<PlanCredit>().Property(p => p.Sum)
                .HasPrecision(12, 2);
            modelBuilder.Entity<PlanDebit>().Property(p => p.Sum)
                .HasPrecision(12, 2);
            modelBuilder.Entity<PlanCreditF2>().Property(p => p.Sum)
                .HasPrecision(12, 2);
            modelBuilder.Entity<PlanDebitF2>().Property(p => p.Sum)
                .HasPrecision(12, 2);

            modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id).ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId).ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId }).ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("AspNetUserClaims");

            modelBuilder.Entity<ActualDebit>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.ActualDebit)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanCredit>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.PlanCredit)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanDebit>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.PlanDebit)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanCreditF2>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.PlanCreditF2)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanDebitF2>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.PlanDebitF2)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<UsrWksRelation>()
               .HasRequired(m => m.ApplicationUser)
               .WithMany(m => m.UsrWksRelation)
               .HasForeignKey(m => m.UserId)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.AccountsBalance)
                .WithRequired(e => e.Account)
                .HasForeignKey(e => e.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.Account)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountsBalance>()
            .HasRequired(m => m.ApplicationUser)
            .WithMany(m => m.AccountsBalance)
            .HasForeignKey(m => m.UserId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanDebit>()
            .HasRequired(m => m.PlanningPeriod)
            .WithMany(m => m.PlanDebit)
            .HasForeignKey(m => m.PeriodId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanCredit>()
            .HasRequired(m => m.PlanningPeriod)
            .WithMany(m => m.PlanCredit)
            .HasForeignKey(m => m.PeriodId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanDebitF2>()
            .HasRequired(m => m.PlanningPeriod)
            .WithMany(m => m.PlanDebitF2)
            .HasForeignKey(m => m.PeriodId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PlanCreditF2>()
            .HasRequired(m => m.PlanningPeriod)
            .WithMany(m => m.PlanCreditF2)
            .HasForeignKey(m => m.PeriodId)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<Settings>()
           .HasRequired(m => m.ApplicationUser)
           .WithMany(m => m.Settings)
           .HasForeignKey(m => m.UserId)
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<Feedback>()
           .HasRequired(m => m.ApplicationUser)
           .WithMany(m => m.Feedback)
           .HasForeignKey(m => m.UserId)
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<Feedback>()
           .HasOptional(m => m.ApplicationUser1)
           .WithMany(m => m.Feedback1)
           .HasForeignKey(m => m.ApprovedUserId)
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<Balance>()
           .HasRequired(m => m.ApplicationUser)
           .WithMany(m => m.Balance)
           .HasForeignKey(m => m.UserId)
           .WillCascadeOnDelete(false);
        }

        public DbSet<F_Result.Models.Payments> Payments { get; set; }
        public DbSet<F_Result.Models.PaymentsF2> PaymentsF2 { get; set; }
        public DbSet<F_Result.Models.ActualDebitsF1> ActualDebitsF1 { get; set; }
        public DbSet<F_Result.Models.ActualDebitsF2> ActualDebitsF2 { get; set; }
        public DbSet<F_Result.Models.Projects> Projects { get; set; }
        public DbSet<F_Result.Models.Organizations> Organizations { get; set; }
        public DbSet<F_Result.Models.DUsers> DUsers { get; set; }
        public DbSet<F_Result.Models.Workers> Workers { get; set; }
        public DbSet<F_Result.Models.Expenditure> Expenditures { get; set; }
        public DbSet<F_Result.Models.Income> Incomes { get; set; }

        public DbSet<AspVisitor> AspVisitors { get; set; }
        public DbSet<F_Result.Models.ApplicationUser> IdentityUsers { get; set; }

        public DbSet<F_Result.Models.ActualDebit> ActualDebit { get; set; }
        public DbSet<F_Result.Models.PlanCredit> PlanCredits { get; set; }
        public DbSet<F_Result.Models.PlanDebit> PlanDebits { get; set; }
        public DbSet<F_Result.Models.PlanCreditF2> PlanCreditsF2 { get; set; }
        public DbSet<F_Result.Models.PlanDebitF2> PlanDebitsF2 { get; set; }
        public DbSet<F_Result.Models.Account> Accounts { get; set; }
        public DbSet<F_Result.Models.AccountsBalance> AccountsBalances { get; set; }
        public DbSet<F_Result.Models.PlanningPeriod> PlanningPeriods { get; set; }
        public DbSet<F_Result.Models.UsrWksRelation> UsrWksRelations { get; set; }
        public DbSet<F_Result.Models.Settings> Settings { get; set; }
        public DbSet<F_Result.Models.Feedback> Feedback { get; set; }
        public DbSet<F_Result.Models.VAspUsers> VAspUsers { get; set; }
        public DbSet<F_Result.Models.ActivityIndex> ActivityIndexes { get; set; }
        public DbSet<F_Result.Models.AAOReport> AAOReports { get; set; }
        public DbSet<F_Result.Models.AAOReportCons> AAOReportCons { get; set; }
        public DbSet<F_Result.Models.Balance> Balance { get; set; }

    }
}

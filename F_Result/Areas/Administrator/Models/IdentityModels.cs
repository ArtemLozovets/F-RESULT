using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;


namespace F_Result.Models
{

    public sealed class ConfigurationA : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public ConfigurationA()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "F_Result.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext db)
        {
        }
    }

    public class ApplicationUser : IdentityUser
    {

        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        public string MiddleName { get; set; }

        [Display(Name = "Должность")]
        public string Post { get; set; }

        [Display(Name = "Роль")]
        public string UserRole { get; set; }

        [NotMapped]
        [Display(Name = "Роль")]
        public string UserRoleName { get; set; }

        public virtual ICollection<ActualDebit> ActualDebit { get; set; }
        public virtual ICollection<PlanCredit> PlanCredit { get; set; }
        public virtual ICollection<PlanDebit> PlanDebit { get; set; }
        public virtual ICollection<PlanCreditF2> PlanCreditF2 { get; set; }
        public virtual ICollection<PlanDebitF2> PlanDebitF2 { get; set; }
        public virtual ICollection<Account> Account { get; set; }
        public virtual ICollection<AccountsBalance> AccountsBalance { get; set; }
        public virtual ICollection<Feedback> Feedback { get; set; }
        public virtual ICollection<Feedback> Feedback1 { get; set; }
        public virtual ICollection<Balance> Balance { get; set; }
        public virtual ICollection<UsrWksRelation> UsrWksRelation { get; set; }
        public virtual ICollection<Settings> Settings { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("FirstName", this.FirstName));
            userIdentity.AddClaim(new Claim("LastName", this.LastName));
            userIdentity.AddClaim(new Claim("MiddleName", this.MiddleName));
            userIdentity.AddClaim(new Claim("Post", this.Post));
            userIdentity.AddClaim(new Claim("UserRole", this.UserRole));

            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole(string name)
            : base(name)
        { }

        public ApplicationRole()
        { }

        public string Description { get; set; }
    }

    class AppContextInitializer : MigrateDatabaseToLatestVersion<ApplicationDbContext, ConfigurationA>
    {

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        static ApplicationDbContext()
        {
            // Database.SetInitializer<ApplicationDbContext>(new AppContextInitializer());
        }
        public ApplicationDbContext()
            : base("FResult", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

    }
}
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;


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

    [Table("AspVisitor")]
    public class AspVisitor
    {
        public int Id { get; set; }

        [Display(Name = "�����")]
        public string Login { get; set; }

        [Display(Name = "������")]
        public string Password { get; set; }

        [Display(Name = "��")]
        public string Ip { get; set; }

        [Display(Name = "�������")]
        public string Url { get; set; }

        [Display(Name = "���������")]
        public bool? Result { get; set; }

        [Display(Name = "����/�����")]
        public DateTime Date { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "���")]
        public string FirstName { get; set; }

        [Display(Name = "�������")]
        public string LastName { get; set; }

        [Display(Name = "��������")]
        public string MiddleName { get; set; }

        [Display(Name = "���������")]
        public string Post { get; set; }

        [Display(Name = "����")]
        public string UserRole { get; set; }

        [NotMapped]
        [Display(Name = "����")]
        public string UserRoleName { get; set; }

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
            Database.SetInitializer<ApplicationDbContext>(new AppContextInitializer());
        }
        public ApplicationDbContext()
            : base("FResult", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //public System.Data.Entity.DbSet<F_Result.Models.ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<AspVisitor> AspVisitors { get; set; }
    }
}
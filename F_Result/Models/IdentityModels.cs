using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations;


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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Post { get; set; }
        public string UserRole { get; set; }

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
    }
}
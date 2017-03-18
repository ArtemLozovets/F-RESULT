using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(F_Result.Startup))]
namespace F_Result
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

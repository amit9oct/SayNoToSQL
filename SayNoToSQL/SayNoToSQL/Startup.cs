using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SayNoToSQL.Startup))]
namespace SayNoToSQL
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

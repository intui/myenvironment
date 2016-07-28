using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(myenvironmentService.Startup))]

namespace myenvironmentService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}
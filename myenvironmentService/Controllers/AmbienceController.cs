using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using myenvironmentService.DataObjects;
using myenvironmentService.Models;

namespace myenvironmentService.Controllers
{
    public class AmbienceController : TableController<Ambience>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            myenvironmentContext context = new myenvironmentContext();
            DomainManager = new EntityDomainManager<Ambience>(context, Request);
        }

        // GET tables/Ambience
        public IQueryable<Ambience> GetAllAmbienceItems()
        {
            return Query();
        }

        // GET tables/Ambience/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Ambience> GetAmbienceItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Ambience/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Ambience> PatchAmbienceItem(string id, Delta<Ambience> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Ambience
        public async Task<IHttpActionResult> PostAmbienceItem(Ambience item)
        {
            Ambience current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Ambience/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteAmbienceItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}
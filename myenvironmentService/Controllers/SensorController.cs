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
    public class SensorController : TableController<Sensor>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            myenvironmentContext context = new myenvironmentContext();
            DomainManager = new EntityDomainManager<Sensor>(context, Request);
        }

        // GET tables/Sensor
        public IQueryable<Sensor> GetAllSensorItems()
        {
            return Query();
        }

        // GET tables/SensorItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Sensor> GetSensorItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/SensorItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Sensor> PatchSensorItem(string id, Delta<Sensor> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/SensorItem
        public async Task<IHttpActionResult> PostSensorItem(Sensor item)
        {
            Sensor current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/SensorItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteSensorItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}
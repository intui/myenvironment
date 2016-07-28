using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.Mobile.Server;

namespace myenvironmentService.DataObjects
{
    public class Sensor : EntityData
    {
        public Guid SensorId { get; set; }
        //[MaxLength(64)]
        public string Title { get; set; }
        public string Location { get; set; }
        public List<Ambience> CurrentAmbientData { get; set; }
    }
}
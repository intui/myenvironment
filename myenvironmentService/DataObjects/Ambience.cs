using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Mobile.Server;

namespace myenvironmentService.DataObjects
{
    public class Ambience : EntityData
    {
        public int AmbienceId { get; set; }
        //[Required]
        public DateTime captureTime { get; set; }
        public decimal temperature { get; set; }
        public decimal humidity { get; set; }
        public Guid SensorId { get; set; }
        public Sensor Sensor { get; set; }
    }
}
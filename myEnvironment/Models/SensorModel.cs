using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace myEnvironment.Models
{
    class SensorModel
    {

        public class SensorContext : DbContext

        {
            public DbSet<Sensor> Sensors { get; set; }
            public DbSet<Ambience> AmbientDataSample { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Filename=Sensors.db");
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Make SensorId required
                modelBuilder.Entity<Sensor>()
                    .Property(b => b.SensorId)
                    .IsRequired();
                // Make required
                modelBuilder.Entity<Ambience>()
                    .Property(b => b.AmbienceId)
                    .IsRequired();
                    //.ValueGeneratedNever();
            }
        }
        // These classes can be declared in individual files. Shown here for simplicity.
        public class Sensor
        {
            public Guid SensorId { get; set; }
            [MaxLength(64)]
            public string Title { get; set; }
            public string Location { get; set; }
            public List<Ambience> CurrentAmbientData { get; set; }
        }
        public class Ambience
        {
            public int AmbienceId { get; set; }
            [Required]
            public DateTime captureTime { get; set; }
            public decimal temperature { get; set; }
            public decimal humidity { get; set; }
            public Guid SensorId { get; set; }
            public Sensor Sensor { get; set; }
        }
        //Read more at https://blogs.windows.com/buildingapps/2016/05/03/data-access-in-universal-windows-platform-uwp-apps/#W3KPxe55g1P4010H.99
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace myEnvironment.Models
{
    class enviroModel
    {
        public class BloggingContext : DbContext
        {
            public DbSet<EnvSetup> Setup { get; set; }
            public DbSet<EnvDataset> Posts { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Filename=EnviroData.db");
            }
        }
        public class EnvDataset
        {
            public int EnvDatasetId { get; set; }
            public DateTime StimeStamp { get; set; }
            public decimal Temperature { get; set; }
            public decimal Humidity { get; set; }

            public EnvSetup Setup { get; set; }
        }

        public class EnvSetup
        {
            public int Id { get; set; }
            public string SensorType { get; set; }
            public string Adress { get; set; }
            
            public DateTime InstallDate { get; set; }
            public List<EnvDataset> DataSets { get; set; }
        }
    }
}

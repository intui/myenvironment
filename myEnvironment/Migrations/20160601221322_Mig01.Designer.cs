using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using myEnvironment.Models;

namespace myEnvironment.Migrations
{
    [DbContext(typeof(SensorModel.SensorContext))]
    [Migration("20160601221322_Mig01")]
    partial class Mig01
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20896");

            modelBuilder.Entity("myEnvironment.Models.SensorModel+Ambience", b =>
                {
                    b.Property<int>("AmbienceId")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("SensorId");

                    b.Property<DateTime>("captureTime");

                    b.Property<decimal>("humidity");

                    b.Property<decimal>("temperature");

                    b.HasKey("AmbienceId");

                    b.HasIndex("SensorId");

                    b.ToTable("AmbientDataSample");
                });

            modelBuilder.Entity("myEnvironment.Models.SensorModel+Sensor", b =>
                {
                    b.Property<Guid>("SensorId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Location");

                    b.Property<string>("Title")
                        .HasAnnotation("MaxLength", 64);

                    b.HasKey("SensorId");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("myEnvironment.Models.SensorModel+Ambience", b =>
                {
                    b.HasOne("myEnvironment.Models.SensorModel+Sensor")
                        .WithMany()
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}

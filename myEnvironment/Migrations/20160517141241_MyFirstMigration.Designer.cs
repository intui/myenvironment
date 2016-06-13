using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using myEnvironment.Models;

namespace myEnvironment.Migrations
{
    [DbContext(typeof(enviroModel.BloggingContext))]
    [Migration("20160517141241_MyFirstMigration")]
    partial class MyFirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20896");

            modelBuilder.Entity("myEnvironment.Models.enviroModel+EnvDataset", b =>
                {
                    b.Property<int>("EnvDatasetId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Humidity");

                    b.Property<int?>("SetupId");

                    b.Property<DateTime>("StimeStamp");

                    b.Property<decimal>("Temperature");

                    b.HasKey("EnvDatasetId");

                    b.HasIndex("SetupId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("myEnvironment.Models.enviroModel+EnvSetup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Adress");

                    b.Property<DateTime>("InstallDate");

                    b.Property<string>("SensorType");

                    b.HasKey("Id");

                    b.ToTable("Setup");
                });

            modelBuilder.Entity("myEnvironment.Models.enviroModel+EnvDataset", b =>
                {
                    b.HasOne("myEnvironment.Models.enviroModel+EnvSetup")
                        .WithMany()
                        .HasForeignKey("SetupId");
                });
        }
    }
}

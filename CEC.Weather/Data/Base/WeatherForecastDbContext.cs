/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.EntityFrameworkCore;
using System;
using CEC.Blazor.Extensions;

namespace CEC.Weather.Data
{
    /// <summary>
    /// Specific <see cref="DbContext"/> implementation for the Weather Database
    /// EF only contains the Readonly DBSet operations through Database Views
    /// CRUD operations are run from Stored Procedures defined in a set of extensions in <see cref="DbContextExtensions"/>.
    /// </summary>
    public class WeatherForecastDbContext : DbContext
    {
        /// <summary>
        /// Tracking lifetime of contexts.
        /// </summary>
        private readonly Guid _id;

        /// <summary>
        /// New Method - creates a guid in case we need to track it
        /// </summary>
        /// <param name="options"></param>
        public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options)
            : base(options)
            => _id = Guid.NewGuid();

        /// <summary>
        /// Generic DbSet to hold distinct Values of a specific value for lists
        /// used in conjuction with <see cref="DbDistinctRequest"/> 
        /// Not created in OnModelCreating.  Built directly from a SQL query
        /// </summary>
        public DbSet<DbDistinct> DistinctList { get; set; }

        /// <summary>
        /// DbSet for the <see cref="DbWeatherForecast"/> record
        /// </summary>
        public DbSet<DbWeatherForecast> WeatherForecast { get; set; }

        /// <summary>
        /// DbSet for the <see cref="DbWeatherStation"/> record
        /// </summary>
        public DbSet<DbWeatherStation> WeatherStation { get; set; }

        /// <summary>
        /// DbSet for the <see cref="DbWeatherReport"/> record
        /// </summary>
        public DbSet<DbWeatherReport> WeatherReport { get; set; }

        /// <summary>
        /// Builds the DbSets from Database Views
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<DbWeatherForecast>(eb =>
                {
                    eb.HasNoKey();
                    eb.ToView("vw_WeatherForecast");
                });
            modelBuilder
                .Entity<DbWeatherStation>(eb =>
                {
                    eb.HasNoKey();
                    eb.ToView("vw_WeatherStation");
                });
            modelBuilder
                .Entity<DbWeatherReport>(eb =>
                {
                    eb.HasNoKey();
                    eb.ToView("vw_WeatherReport");
                });
        }
    }
}

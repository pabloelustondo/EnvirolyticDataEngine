using System.Data.Entity;

namespace JassWeather.Models
{
    public class JassWeatherContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<JassWeather.Models.JassWeatherContext>());

        public JassWeatherContext() : base("name=JassWeatherContext")
        {
        }

        public DbSet<APIRequestSet> APIRequestSets { get; set; }
        public DbSet<APIRequest> APIRequests { get; set; }

        public DbSet<JassMeasure> JassMeasures { get; set; }

        public DbSet<JassVariable> JassVariables { get; set; }

        public DbSet<JassPartition> JassPartitions { get; set; }

        public DbSet<JassGrid> JassGrids { get; set; }

        public DbSet<JassBuilder> JassBuilders { get; set; }

        public DbSet<JassBuilderLog> JassBuilderLogs { get; set; }

        public DbSet<JassFormula> JassFormulas { get; set; }

        public DbSet<JassDeriver> JassDerivers { get; set; }

        public DbSet<JassLatLon> JassLatLons { get; set; }

        public DbSet<JassLatLonGroup> JassLatLonGroups { get; set; }

    }
}

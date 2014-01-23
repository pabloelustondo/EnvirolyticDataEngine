namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class historicalfield : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequestSets", "HistoricalCurrentForecast", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequestSets", "HistoricalCurrentForecast");
        }
    }
}

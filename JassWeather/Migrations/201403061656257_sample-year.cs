namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sampleyear : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "sampleYear", c => c.Int());
            AddColumn("dbo.APIRequests", "sampleMonth", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "sampleMonth");
            DropColumn("dbo.APIRequests", "sampleYear");
        }
    }
}

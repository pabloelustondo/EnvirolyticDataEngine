namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loadDelay : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "startLoadTime", c => c.DateTime());
            AddColumn("dbo.APIRequests", "endLoadTime", c => c.DateTime());
            AddColumn("dbo.APIRequests", "spanLoadTime", c => c.Time());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "spanLoadTime");
            DropColumn("dbo.APIRequests", "endLoadTime");
            DropColumn("dbo.APIRequests", "startLoadTime");
        }
    }
}

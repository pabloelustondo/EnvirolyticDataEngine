namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class timespan : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "startGetTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.APIRequests", "endGetTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.APIRequests", "spanGetTime", c => c.Time(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "spanGetTime");
            DropColumn("dbo.APIRequests", "endGetTime");
            DropColumn("dbo.APIRequests", "startGetTime");
        }
    }
}

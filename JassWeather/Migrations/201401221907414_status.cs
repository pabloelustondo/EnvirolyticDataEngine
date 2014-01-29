namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class status : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "status");
        }
    }
}

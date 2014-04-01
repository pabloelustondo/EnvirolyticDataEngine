namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hrdifference : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "hrDifference", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassLatLons", "hrDifference");
        }
    }
}

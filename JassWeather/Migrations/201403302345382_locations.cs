namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class locations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "narrY", c => c.Int(nullable: false));
            AddColumn("dbo.JassLatLons", "narrX", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassLatLons", "narrX");
            DropColumn("dbo.JassLatLons", "narrY");
        }
    }
}

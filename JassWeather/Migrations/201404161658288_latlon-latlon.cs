namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latlonlatlon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "narrLat", c => c.Double());
            AddColumn("dbo.JassLatLons", "narrLon", c => c.Double());
            AddColumn("dbo.JassLatLons", "maccLat", c => c.Double());
            AddColumn("dbo.JassLatLons", "maccLon", c => c.Double());
            AddColumn("dbo.JassLatLons", "csfrLat", c => c.Double());
            AddColumn("dbo.JassLatLons", "csfrLon", c => c.Double());
            AddColumn("dbo.JassLatLons", "sherLat", c => c.Double());
            AddColumn("dbo.JassLatLons", "sherLon", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassLatLons", "sherLon");
            DropColumn("dbo.JassLatLons", "sherLat");
            DropColumn("dbo.JassLatLons", "csfrLon");
            DropColumn("dbo.JassLatLons", "csfrLat");
            DropColumn("dbo.JassLatLons", "maccLon");
            DropColumn("dbo.JassLatLons", "maccLat");
            DropColumn("dbo.JassLatLons", "narrLon");
            DropColumn("dbo.JassLatLons", "narrLat");
        }
    }
}

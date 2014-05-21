namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aaa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "napsNO2Y", c => c.Int());
            AddColumn("dbo.JassLatLons", "napsNO2X", c => c.Int());
            AddColumn("dbo.JassLatLons", "napsNO2Lat", c => c.Double());
            AddColumn("dbo.JassLatLons", "napsNO2Lon", c => c.Double());
            AddColumn("dbo.JassLatLons", "napsO3Y", c => c.Int());
            AddColumn("dbo.JassLatLons", "napsO3X", c => c.Int());
            AddColumn("dbo.JassLatLons", "napsO3Lat", c => c.Double());
            AddColumn("dbo.JassLatLons", "napsO3Lon", c => c.Double());
            AddColumn("dbo.JassLatLons", "napsPM25Y", c => c.Int());
            AddColumn("dbo.JassLatLons", "napsPM25X", c => c.Int());
            AddColumn("dbo.JassLatLons", "napsPM25Lat", c => c.Double());
            AddColumn("dbo.JassLatLons", "napsPM25Lon", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassLatLons", "napsPM25Lon");
            DropColumn("dbo.JassLatLons", "napsPM25Lat");
            DropColumn("dbo.JassLatLons", "napsPM25X");
            DropColumn("dbo.JassLatLons", "napsPM25Y");
            DropColumn("dbo.JassLatLons", "napsO3Lon");
            DropColumn("dbo.JassLatLons", "napsO3Lat");
            DropColumn("dbo.JassLatLons", "napsO3X");
            DropColumn("dbo.JassLatLons", "napsO3Y");
            DropColumn("dbo.JassLatLons", "napsNO2Lon");
            DropColumn("dbo.JassLatLons", "napsNO2Lat");
            DropColumn("dbo.JassLatLons", "napsNO2X");
            DropColumn("dbo.JassLatLons", "napsNO2Y");
        }
    }
}

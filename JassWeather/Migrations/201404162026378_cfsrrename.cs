namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cfsrrename : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "cfsrY", c => c.Int());
            AddColumn("dbo.JassLatLons", "cfsrX", c => c.Int());
            AddColumn("dbo.JassLatLons", "cfsrLat", c => c.Double());
            AddColumn("dbo.JassLatLons", "cfsrLon", c => c.Double());
            DropColumn("dbo.JassLatLons", "csfrY");
            DropColumn("dbo.JassLatLons", "csfrX");
            DropColumn("dbo.JassLatLons", "csfrLat");
            DropColumn("dbo.JassLatLons", "csfrLon");
        }
        
        public override void Down()
        {
            AddColumn("dbo.JassLatLons", "csfrLon", c => c.Double());
            AddColumn("dbo.JassLatLons", "csfrLat", c => c.Double());
            AddColumn("dbo.JassLatLons", "csfrX", c => c.Int());
            AddColumn("dbo.JassLatLons", "csfrY", c => c.Int());
            DropColumn("dbo.JassLatLons", "cfsrLon");
            DropColumn("dbo.JassLatLons", "cfsrLat");
            DropColumn("dbo.JassLatLons", "cfsrX");
            DropColumn("dbo.JassLatLons", "cfsrY");
        }
    }
}

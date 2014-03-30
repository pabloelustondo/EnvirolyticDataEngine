namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latlonrelationwithgroups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "JassLatLonGroupID", c => c.Int());
            AddForeignKey("dbo.JassLatLons", "JassLatLonGroupID", "dbo.JassLatLonGroups", "JassLatLonGroupID");
            CreateIndex("dbo.JassLatLons", "JassLatLonGroupID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassLatLons", new[] { "JassLatLonGroupID" });
            DropForeignKey("dbo.JassLatLons", "JassLatLonGroupID", "dbo.JassLatLonGroups");
            DropColumn("dbo.JassLatLons", "JassLatLonGroupID");
        }
    }
}

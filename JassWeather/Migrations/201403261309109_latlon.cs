namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latlon : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassLatLons",
                c => new
                    {
                        JassLatLonID = c.Int(nullable: false, identity: true),
                        StationCode = c.String(),
                        Info = c.String(),
                        Lat = c.Double(nullable: false),
                        Lon = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.JassLatLonID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JassLatLons");
        }
    }
}

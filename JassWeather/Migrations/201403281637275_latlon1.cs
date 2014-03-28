namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latlon1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassLatLonGroups",
                c => new
                    {
                        JassLatLonGroupID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.JassLatLonGroupID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JassLatLonGroups");
        }
    }
}

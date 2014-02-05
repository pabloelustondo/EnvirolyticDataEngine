namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class measures : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassMeasures",
                c => new
                    {
                        JassMeasureID = c.Int(nullable: false, identity: true),
                        x = c.Int(nullable: false),
                        y = c.Int(nullable: false),
                        day = c.Int(nullable: false),
                        hour3 = c.Int(nullable: false),
                        level = c.String(),
                    })
                .PrimaryKey(t => t.JassMeasureID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JassMeasures");
        }
    }
}

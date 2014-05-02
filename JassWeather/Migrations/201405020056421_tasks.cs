namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tasks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassTasks",
                c => new
                    {
                        JassTaskID = c.Int(nullable: false, identity: true),
                        taskName = c.String(),
                    })
                .PrimaryKey(t => t.JassTaskID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JassTasks");
        }
    }
}

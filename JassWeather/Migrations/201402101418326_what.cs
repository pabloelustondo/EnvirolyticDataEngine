namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class what : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassBuilders",
                c => new
                    {
                        JassBuilderID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        JassVariableID = c.Int(nullable: false),
                        JassGridID = c.Int(nullable: false),
                        APIRequestId = c.Int(nullable: false),
                        x = c.Int(),
                        y = c.Int(),
                        year = c.Int(),
                        month = c.Int(),
                        day = c.Int(),
                        hour3 = c.Int(),
                        level = c.Int(),
                    })
                .PrimaryKey(t => t.JassBuilderID)
                .ForeignKey("dbo.JassVariables", t => t.JassVariableID, cascadeDelete: true)
                .ForeignKey("dbo.JassGrids", t => t.JassGridID, cascadeDelete: true)
                .ForeignKey("dbo.APIRequests", t => t.APIRequestId, cascadeDelete: true)
                .Index(t => t.JassVariableID)
                .Index(t => t.JassGridID)
                .Index(t => t.APIRequestId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassBuilders", new[] { "APIRequestId" });
            DropIndex("dbo.JassBuilders", new[] { "JassGridID" });
            DropIndex("dbo.JassBuilders", new[] { "JassVariableID" });
            DropForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequests");
            DropForeignKey("dbo.JassBuilders", "JassGridID", "dbo.JassGrids");
            DropForeignKey("dbo.JassBuilders", "JassVariableID", "dbo.JassVariables");
            DropTable("dbo.JassBuilders");
        }
    }
}

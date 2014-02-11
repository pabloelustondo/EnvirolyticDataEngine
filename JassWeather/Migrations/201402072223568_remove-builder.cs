namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removebuilder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassBuilders", "JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassBuilders", "JassGridID", "dbo.JassGrids");
            DropForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequests");
            DropIndex("dbo.JassBuilders", new[] { "JassVariableID" });
            DropIndex("dbo.JassBuilders", new[] { "JassGridID" });
            DropIndex("dbo.JassBuilders", new[] { "APIRequestId" });
            DropTable("dbo.JassBuilders");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.JassBuilderID);
            
            CreateIndex("dbo.JassBuilders", "APIRequestId");
            CreateIndex("dbo.JassBuilders", "JassGridID");
            CreateIndex("dbo.JassBuilders", "JassVariableID");
            AddForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.JassBuilders", "JassGridID", "dbo.JassGrids", "JassGridID", cascadeDelete: true);
            AddForeignKey("dbo.JassBuilders", "JassVariableID", "dbo.JassVariables", "JassVariableID", cascadeDelete: true);
        }
    }
}

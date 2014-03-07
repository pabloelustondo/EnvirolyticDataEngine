namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class builderlog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassBuilderLogs",
                c => new
                    {
                        JassBuilderLogID = c.Int(nullable: false, identity: true),
                        JassBuilderID = c.Int(nullable: false),
                        ParentJassBuilderLogID = c.Int(),
                        EventType = c.String(),
                        Message = c.String(),
                        Success = c.Boolean(nullable: false),
                        year = c.Int(),
                        month = c.Int(),
                        day = c.Int(),
                        yearEnd = c.Int(),
                        monthEnd = c.Int(),
                        startTotalTime = c.DateTime(),
                        endTotalTime = c.DateTime(),
                        spanTotalTime = c.Time(),
                        ParentJassBuilderLog_JassBuilderLogID = c.Int(),
                    })
                .PrimaryKey(t => t.JassBuilderLogID)
                .ForeignKey("dbo.JassBuilders", t => t.JassBuilderID, cascadeDelete: true)
                .ForeignKey("dbo.JassBuilderLogs", t => t.ParentJassBuilderLog_JassBuilderLogID)
                .Index(t => t.JassBuilderID)
                .Index(t => t.ParentJassBuilderLog_JassBuilderLogID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassBuilderLogs", new[] { "ParentJassBuilderLog_JassBuilderLogID" });
            DropIndex("dbo.JassBuilderLogs", new[] { "JassBuilderID" });
            DropForeignKey("dbo.JassBuilderLogs", "ParentJassBuilderLog_JassBuilderLogID", "dbo.JassBuilderLogs");
            DropForeignKey("dbo.JassBuilderLogs", "JassBuilderID", "dbo.JassBuilders");
            DropTable("dbo.JassBuilderLogs");
        }
    }
}

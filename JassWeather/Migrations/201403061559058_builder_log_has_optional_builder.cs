namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class builder_log_has_optional_builder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassBuilderLogs", "JassBuilderID", "dbo.JassBuilders");
            DropIndex("dbo.JassBuilderLogs", new[] { "JassBuilderID" });
            AlterColumn("dbo.JassBuilderLogs", "JassBuilderID", c => c.Int());
            AddForeignKey("dbo.JassBuilderLogs", "JassBuilderID", "dbo.JassBuilders", "JassBuilderID");
            CreateIndex("dbo.JassBuilderLogs", "JassBuilderID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassBuilderLogs", new[] { "JassBuilderID" });
            DropForeignKey("dbo.JassBuilderLogs", "JassBuilderID", "dbo.JassBuilders");
            AlterColumn("dbo.JassBuilderLogs", "JassBuilderID", c => c.Int(nullable: false));
            CreateIndex("dbo.JassBuilderLogs", "JassBuilderID");
            AddForeignKey("dbo.JassBuilderLogs", "JassBuilderID", "dbo.JassBuilders", "JassBuilderID", cascadeDelete: true);
        }
    }
}

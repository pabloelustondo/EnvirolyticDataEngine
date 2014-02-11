namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class request : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequestSets");
            DropIndex("dbo.JassBuilders", new[] { "APIRequestId" });
            AddForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequests", "Id", cascadeDelete: true);
            CreateIndex("dbo.JassBuilders", "APIRequestId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassBuilders", new[] { "APIRequestId" });
            DropForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequests");
            CreateIndex("dbo.JassBuilders", "APIRequestId");
            AddForeignKey("dbo.JassBuilders", "APIRequestId", "dbo.APIRequestSets", "Id", cascadeDelete: true);
        }
    }
}

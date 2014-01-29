namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.APIRequestSets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        description = c.String(),
                        priority = c.Int(nullable: false),
                        dataAccessLocation = c.String(),
                        dataSetUpdates = c.String(),
                        temporalResolution = c.String(),
                        fileType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.APIRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        APIRequestSetId = c.Int(nullable: false),
                        url = c.String(),
                        name = c.String(),
                        type = c.String(),
                        schedule = c.String(),
                        variable = c.String(),
                        variableConsolidated = c.String(),
                        impactHealth = c.Int(nullable: false),
                        impactBusiness = c.Int(nullable: false),
                        impactConsumer = c.Int(nullable: false),
                        impactAgriculture = c.Int(nullable: false),
                        geographicResolution = c.String(),
                        fileFormat = c.String(),
                        fileSizeGB = c.Int(nullable: false),
                        description = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.APIRequestSets", t => t.APIRequestSetId, cascadeDelete: true)
                .Index(t => t.APIRequestSetId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.APIRequests", new[] { "APIRequestSetId" });
            DropForeignKey("dbo.APIRequests", "APIRequestSetId", "dbo.APIRequestSets");
            DropTable("dbo.APIRequests");
            DropTable("dbo.APIRequestSets");
        }
    }
}

namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class somebasdtypechecking : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.APIRequests", "startGetTime", c => c.DateTime());
            AlterColumn("dbo.APIRequests", "endGetTime", c => c.DateTime());
            AlterColumn("dbo.APIRequests", "spanGetTime", c => c.Time());
            AlterColumn("dbo.APIRequests", "fileSize", c => c.Int());
            AlterColumn("dbo.APIRequests", "impactHealth", c => c.Int());
            AlterColumn("dbo.APIRequests", "impactBusiness", c => c.Int());
            AlterColumn("dbo.APIRequests", "impactConsumer", c => c.Int());
            AlterColumn("dbo.APIRequests", "impactAgriculture", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.APIRequests", "impactAgriculture", c => c.Int(nullable: false));
            AlterColumn("dbo.APIRequests", "impactConsumer", c => c.Int(nullable: false));
            AlterColumn("dbo.APIRequests", "impactBusiness", c => c.Int(nullable: false));
            AlterColumn("dbo.APIRequests", "impactHealth", c => c.Int(nullable: false));
            AlterColumn("dbo.APIRequests", "fileSize", c => c.Int(nullable: false));
            AlterColumn("dbo.APIRequests", "spanGetTime", c => c.Time(nullable: false));
            AlterColumn("dbo.APIRequests", "endGetTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.APIRequests", "startGetTime", c => c.DateTime(nullable: false));
        }
    }
}

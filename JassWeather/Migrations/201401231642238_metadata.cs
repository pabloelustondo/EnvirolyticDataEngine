namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class metadata : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "fileSize", c => c.Int(nullable: false));
            AddColumn("dbo.APIRequests", "zenType", c => c.String());
            AddColumn("dbo.APIRequests", "dataSource", c => c.String());
            AddColumn("dbo.APIRequests", "isHistorical", c => c.Boolean(nullable: false));
            AddColumn("dbo.APIRequests", "isCurrent", c => c.Boolean(nullable: false));
            AddColumn("dbo.APIRequests", "isForecast", c => c.Boolean(nullable: false));
            AddColumn("dbo.APIRequests", "frecuency", c => c.String());
            AddColumn("dbo.APIRequests", "typeOfMeasure", c => c.String());
            AddColumn("dbo.APIRequests", "cost", c => c.String());
            DropColumn("dbo.APIRequests", "fileSizeGB");
        }
        
        public override void Down()
        {
            AddColumn("dbo.APIRequests", "fileSizeGB", c => c.Int(nullable: false));
            DropColumn("dbo.APIRequests", "cost");
            DropColumn("dbo.APIRequests", "typeOfMeasure");
            DropColumn("dbo.APIRequests", "frecuency");
            DropColumn("dbo.APIRequests", "isForecast");
            DropColumn("dbo.APIRequests", "isCurrent");
            DropColumn("dbo.APIRequests", "isHistorical");
            DropColumn("dbo.APIRequests", "dataSource");
            DropColumn("dbo.APIRequests", "zenType");
            DropColumn("dbo.APIRequests", "fileSize");
        }
    }
}

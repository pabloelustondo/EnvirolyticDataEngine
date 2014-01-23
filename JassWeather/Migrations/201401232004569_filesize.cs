namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class filesize : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.APIRequests", "fileSize", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.APIRequests", "fileSize", c => c.Int(nullable: false));
        }
    }
}

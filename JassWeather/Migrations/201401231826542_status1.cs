namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class status1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "onDisk", c => c.String());
            AddColumn("dbo.APIRequests", "onBlob", c => c.String());
            AddColumn("dbo.APIRequests", "onTable", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "onTable");
            DropColumn("dbo.APIRequests", "onBlob");
            DropColumn("dbo.APIRequests", "onDisk");
        }
    }
}

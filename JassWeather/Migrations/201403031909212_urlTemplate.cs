namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class urlTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "urlTemplate", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "urlTemplate");
        }
    }
}

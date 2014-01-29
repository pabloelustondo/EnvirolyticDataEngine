namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statisticlevel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "statistic", c => c.String());
            AddColumn("dbo.APIRequests", "level", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "level");
            DropColumn("dbo.APIRequests", "statistic");
        }
    }
}

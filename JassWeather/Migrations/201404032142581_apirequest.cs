namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class apirequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "sampleWeeky", c => c.Int());
            AddColumn("dbo.APIRequests", "sampleDay", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "sampleDay");
            DropColumn("dbo.APIRequests", "sampleWeeky");
        }
    }
}

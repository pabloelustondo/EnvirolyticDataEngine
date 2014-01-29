namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APIRequests", "schema", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.APIRequests", "schema");
        }
    }
}

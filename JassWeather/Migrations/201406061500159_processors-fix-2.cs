namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class processorsfix2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassProcessors", "update", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassProcessors", "update");
        }
    }
}

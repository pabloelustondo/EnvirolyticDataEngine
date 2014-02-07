namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassGrids", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassGrids", "Name");
        }
    }
}

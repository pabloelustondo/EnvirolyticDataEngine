namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bs : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.JassGrids", "XName");
            DropColumn("dbo.JassGrids", "YName");
            DropColumn("dbo.JassGrids", "LevelName");
            DropColumn("dbo.JassGrids", "TimeName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.JassGrids", "TimeName", c => c.String());
            AddColumn("dbo.JassGrids", "LevelName", c => c.String());
            AddColumn("dbo.JassGrids", "YName", c => c.String());
            AddColumn("dbo.JassGrids", "XName", c => c.String());
        }
    }
}

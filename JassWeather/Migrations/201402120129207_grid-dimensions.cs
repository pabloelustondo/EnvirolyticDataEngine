namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class griddimensions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassGrids", "XName", c => c.String());
            AddColumn("dbo.JassGrids", "YName", c => c.String());
            AddColumn("dbo.JassGrids", "LevelName", c => c.String());
            AddColumn("dbo.JassGrids", "TimeName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassGrids", "TimeName");
            DropColumn("dbo.JassGrids", "LevelName");
            DropColumn("dbo.JassGrids", "YName");
            DropColumn("dbo.JassGrids", "XName");
        }
    }
}

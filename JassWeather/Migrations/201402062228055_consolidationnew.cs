namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class consolidationnew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassConsolidations", "x", c => c.Int(nullable: false));
            AddColumn("dbo.JassConsolidations", "y", c => c.Int(nullable: false));
            AddColumn("dbo.JassConsolidations", "year", c => c.Int(nullable: false));
            AddColumn("dbo.JassConsolidations", "month", c => c.Int(nullable: false));
            AddColumn("dbo.JassConsolidations", "day", c => c.Int(nullable: false));
            AddColumn("dbo.JassConsolidations", "hour3", c => c.Int(nullable: false));
            AddColumn("dbo.JassConsolidations", "level", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassConsolidations", "level");
            DropColumn("dbo.JassConsolidations", "hour3");
            DropColumn("dbo.JassConsolidations", "day");
            DropColumn("dbo.JassConsolidations", "month");
            DropColumn("dbo.JassConsolidations", "year");
            DropColumn("dbo.JassConsolidations", "y");
            DropColumn("dbo.JassConsolidations", "x");
        }
    }
}

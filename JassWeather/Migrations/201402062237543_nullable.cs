namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.JassConsolidations", "x", c => c.Int());
            AlterColumn("dbo.JassConsolidations", "y", c => c.Int());
            AlterColumn("dbo.JassConsolidations", "year", c => c.Int());
            AlterColumn("dbo.JassConsolidations", "month", c => c.Int());
            AlterColumn("dbo.JassConsolidations", "day", c => c.Int());
            AlterColumn("dbo.JassConsolidations", "hour3", c => c.Int());
            AlterColumn("dbo.JassConsolidations", "level", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JassConsolidations", "level", c => c.Int(nullable: false));
            AlterColumn("dbo.JassConsolidations", "hour3", c => c.Int(nullable: false));
            AlterColumn("dbo.JassConsolidations", "day", c => c.Int(nullable: false));
            AlterColumn("dbo.JassConsolidations", "month", c => c.Int(nullable: false));
            AlterColumn("dbo.JassConsolidations", "year", c => c.Int(nullable: false));
            AlterColumn("dbo.JassConsolidations", "y", c => c.Int(nullable: false));
            AlterColumn("dbo.JassConsolidations", "x", c => c.Int(nullable: false));
        }
    }
}

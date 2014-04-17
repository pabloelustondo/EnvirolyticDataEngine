namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stepsinday : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassGrids", "StepsInDay", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassGrids", "StepsInDay");
        }
    }
}

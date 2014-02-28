namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class builderyearend : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "yearEnd", c => c.Int());
            AddColumn("dbo.JassBuilders", "monthEnd", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "monthEnd");
            DropColumn("dbo.JassBuilders", "yearEnd");
        }
    }
}

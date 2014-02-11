namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aaa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "startTotalTime", c => c.DateTime());
            AddColumn("dbo.JassBuilders", "endTotalTime", c => c.DateTime());
            AddColumn("dbo.JassBuilders", "spanTotalTime", c => c.Time());
            AddColumn("dbo.JassBuilders", "OnDisk", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "OnDisk");
            DropColumn("dbo.JassBuilders", "spanTotalTime");
            DropColumn("dbo.JassBuilders", "endTotalTime");
            DropColumn("dbo.JassBuilders", "startTotalTime");
        }
    }
}

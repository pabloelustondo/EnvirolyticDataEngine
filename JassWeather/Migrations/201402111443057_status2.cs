namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class status2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.JassBuilders", "setTotalSize", c => c.Int(nullable: false));
            AddColumn("dbo.JassBuilders", "setCurrentSize", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "setCurrentSize");
            DropColumn("dbo.JassBuilders", "setTotalSize");
            DropColumn("dbo.JassBuilders", "Status");
        }
    }
}

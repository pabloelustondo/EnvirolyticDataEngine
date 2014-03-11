namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServerName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "ServerName", c => c.String());
            AddColumn("dbo.JassBuilderLogs", "ServerName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilderLogs", "ServerName");
            DropColumn("dbo.JassBuilders", "ServerName");
        }
    }
}

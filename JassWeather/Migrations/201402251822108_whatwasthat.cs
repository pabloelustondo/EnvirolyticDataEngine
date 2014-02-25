namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class whatwasthat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "unpack", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "unpack");
        }
    }
}

namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class weeky : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "weeky", c => c.Int());
            AddColumn("dbo.JassBuilders", "weekyEnd", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "weekyEnd");
            DropColumn("dbo.JassBuilders", "weeky");
        }
    }
}

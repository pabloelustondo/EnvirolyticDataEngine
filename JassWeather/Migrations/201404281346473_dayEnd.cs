namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dayEnd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "dayEnd", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "dayEnd");
        }
    }
}

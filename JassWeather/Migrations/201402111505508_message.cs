namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class message : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "Message", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "Message");
        }
    }
}

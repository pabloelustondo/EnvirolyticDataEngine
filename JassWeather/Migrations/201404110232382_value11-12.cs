namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class value1112 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassColorCodes", "Value11", c => c.Int(nullable: false));
            AddColumn("dbo.JassColorCodes", "Color11", c => c.String());
            AddColumn("dbo.JassColorCodes", "Value12", c => c.Int(nullable: false));
            AddColumn("dbo.JassColorCodes", "Color12", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassColorCodes", "Color12");
            DropColumn("dbo.JassColorCodes", "Value12");
            DropColumn("dbo.JassColorCodes", "Color11");
            DropColumn("dbo.JassColorCodes", "Value11");
        }
    }
}

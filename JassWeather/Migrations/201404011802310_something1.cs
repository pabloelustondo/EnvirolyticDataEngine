namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class something1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.JassLatLons", "narrY", c => c.Int());
            AlterColumn("dbo.JassLatLons", "narrX", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JassLatLons", "narrX", c => c.Int(nullable: false));
            AlterColumn("dbo.JassLatLons", "narrY", c => c.Int(nullable: false));
        }
    }
}

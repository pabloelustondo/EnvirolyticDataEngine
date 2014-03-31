namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latlonname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassLatLons", "Name");
        }
    }
}

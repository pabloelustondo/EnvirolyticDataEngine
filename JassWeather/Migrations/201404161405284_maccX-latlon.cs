namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class maccXlatlon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassLatLons", "maccY", c => c.Int());
            AddColumn("dbo.JassLatLons", "maccX", c => c.Int());
            AddColumn("dbo.JassLatLons", "csfrY", c => c.Int());
            AddColumn("dbo.JassLatLons", "csfrX", c => c.Int());
            AddColumn("dbo.JassLatLons", "sherY", c => c.Int());
            AddColumn("dbo.JassLatLons", "sherX", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassLatLons", "sherX");
            DropColumn("dbo.JassLatLons", "sherY");
            DropColumn("dbo.JassLatLons", "csfrX");
            DropColumn("dbo.JassLatLons", "csfrY");
            DropColumn("dbo.JassLatLons", "maccX");
            DropColumn("dbo.JassLatLons", "maccY");
        }
    }
}

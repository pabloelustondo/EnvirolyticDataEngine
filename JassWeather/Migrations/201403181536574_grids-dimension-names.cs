namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gridsdimensionnames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassGrids", "Xname", c => c.String());
            AddColumn("dbo.JassGrids", "Yname", c => c.String());
            AddColumn("dbo.JassGrids", "Levelname", c => c.String());
            AddColumn("dbo.JassGrids", "Timename", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassGrids", "Timename");
            DropColumn("dbo.JassGrids", "Levelname");
            DropColumn("dbo.JassGrids", "Yname");
            DropColumn("dbo.JassGrids", "Xname");
        }
    }
}

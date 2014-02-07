namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class something : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassVariables", "FileName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassVariables", "FileName");
        }
    }
}

namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hack : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassDerivers", "X1", c => c.String());
            AddColumn("dbo.JassDerivers", "X2", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassDerivers", "X2");
            DropColumn("dbo.JassDerivers", "X1");
        }
    }
}

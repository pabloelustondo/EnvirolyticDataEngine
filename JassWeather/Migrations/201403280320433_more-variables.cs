namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class morevariables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassDerivers", "X3", c => c.String());
            AddColumn("dbo.JassDerivers", "X4", c => c.String());
            AddColumn("dbo.JassDerivers", "X3Level", c => c.Int());
            AddColumn("dbo.JassDerivers", "X4Level", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassDerivers", "X4Level");
            DropColumn("dbo.JassDerivers", "X3Level");
            DropColumn("dbo.JassDerivers", "X4");
            DropColumn("dbo.JassDerivers", "X3");
        }
    }
}

namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class x5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassDerivers", "X5", c => c.String());
            DropColumn("dbo.JassDerivers", "X1HistoryLength");
            DropColumn("dbo.JassDerivers", "X2HistoryLength");
            DropColumn("dbo.JassDerivers", "X3HistoryLength");
        }
        
        public override void Down()
        {
            AddColumn("dbo.JassDerivers", "X3HistoryLength", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2HistoryLength", c => c.Int());
            AddColumn("dbo.JassDerivers", "X1HistoryLength", c => c.Int());
            DropColumn("dbo.JassDerivers", "X5");
        }
    }
}

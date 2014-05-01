namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class historylength : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassDerivers", "X1HistoryLength", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2HistoryLength", c => c.Int());
            AddColumn("dbo.JassDerivers", "X3HistoryLength", c => c.Int());
            AddColumn("dbo.JassDerivers", "X4HistoryLength", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassDerivers", "X4HistoryLength");
            DropColumn("dbo.JassDerivers", "X3HistoryLength");
            DropColumn("dbo.JassDerivers", "X2HistoryLength");
            DropColumn("dbo.JassDerivers", "X1HistoryLength");
        }
    }
}

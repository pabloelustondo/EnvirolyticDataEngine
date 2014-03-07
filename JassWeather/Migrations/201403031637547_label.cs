namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class label : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilderLogs", "Label", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilderLogs", "Label");
        }
    }
}

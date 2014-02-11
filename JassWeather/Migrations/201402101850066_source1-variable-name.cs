namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class source1variablename : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassBuilders", "Source1VariableName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassBuilders", "Source1VariableName");
        }
    }
}

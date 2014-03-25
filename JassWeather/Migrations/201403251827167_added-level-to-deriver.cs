namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedleveltoderiver : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassDerivers", "X1Level", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2Level", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JassDerivers", "X2Level");
            DropColumn("dbo.JassDerivers", "X1Level");
        }
    }
}

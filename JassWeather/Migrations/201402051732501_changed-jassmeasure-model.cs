namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changedjassmeasuremodel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassMeasures", "year", c => c.Int(nullable: false));
            AddColumn("dbo.JassMeasures", "month", c => c.Int(nullable: false));
            AlterColumn("dbo.JassMeasures", "level", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JassMeasures", "level", c => c.String());
            DropColumn("dbo.JassMeasures", "month");
            DropColumn("dbo.JassMeasures", "year");
        }
    }
}

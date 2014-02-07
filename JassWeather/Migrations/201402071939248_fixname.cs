namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixname : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.JassPartitions", "Name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.JassPartitions", "Name", c => c.Int(nullable: false));
        }
    }
}

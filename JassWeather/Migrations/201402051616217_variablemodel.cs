namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class variablemodel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassVariables",
                c => new
                    {
                        JassVariableID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.JassVariableID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JassVariables");
        }
    }
}

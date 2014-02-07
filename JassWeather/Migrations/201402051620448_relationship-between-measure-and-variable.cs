namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class relationshipbetweenmeasureandvariable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassMeasures", "JassVariableID", c => c.Int(nullable: false));
            AddForeignKey("dbo.JassMeasures", "JassVariableID", "dbo.JassVariables", "JassVariableID", cascadeDelete: true);
            CreateIndex("dbo.JassMeasures", "JassVariableID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassMeasures", new[] { "JassVariableID" });
            DropForeignKey("dbo.JassMeasures", "JassVariableID", "dbo.JassVariables");
            DropColumn("dbo.JassMeasures", "JassVariableID");
        }
    }
}

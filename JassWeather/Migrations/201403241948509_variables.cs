namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class variables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables");
            DropIndex("dbo.JassDerivers", new[] { "X1JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "X2JassVariable_JassVariableID" });
            DropColumn("dbo.JassDerivers", "X1JassVariableID");
            DropColumn("dbo.JassDerivers", "X2JassVariableID");
            DropColumn("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            DropColumn("dbo.JassDerivers", "X2JassVariable_JassVariableID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.JassDerivers", "X2JassVariable_JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X1JassVariable_JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2JassVariableID", c => c.Int(nullable: false));
            AddColumn("dbo.JassDerivers", "X1JassVariableID", c => c.Int(nullable: false));
            CreateIndex("dbo.JassDerivers", "X2JassVariable_JassVariableID");
            CreateIndex("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
        }
    }
}

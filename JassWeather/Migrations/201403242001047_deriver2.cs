namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deriver2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JassDerivers", "X1JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X1JassVariable_JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2JassVariable_JassVariableID", c => c.Int());
            AddForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            CreateIndex("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            CreateIndex("dbo.JassDerivers", "X2JassVariable_JassVariableID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassDerivers", new[] { "X2JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "X1JassVariable_JassVariableID" });
            DropForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables");
            DropColumn("dbo.JassDerivers", "X2JassVariable_JassVariableID");
            DropColumn("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            DropColumn("dbo.JassDerivers", "X2JassVariableID");
            DropColumn("dbo.JassDerivers", "X1JassVariableID");
        }
    }
}

namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aaaaaaa : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassDerivers", "JassVariableID", "dbo.JassVariables");
            DropIndex("dbo.JassDerivers", new[] { "JassVariableID" });
            RenameColumn(table: "dbo.JassDerivers", name: "JassVariableID", newName: "JassVariable_JassVariableID");
            AddColumn("dbo.JassDerivers", "X1JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X1JassVariable_JassVariableID", c => c.Int());
            AddColumn("dbo.JassDerivers", "X2JassVariable_JassVariableID", c => c.Int());
            AddForeignKey("dbo.JassDerivers", "JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            CreateIndex("dbo.JassDerivers", "JassVariable_JassVariableID");
            CreateIndex("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            CreateIndex("dbo.JassDerivers", "X2JassVariable_JassVariableID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassDerivers", new[] { "X2JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "X1JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "JassVariable_JassVariableID" });
            DropForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "JassVariable_JassVariableID", "dbo.JassVariables");
            DropColumn("dbo.JassDerivers", "X2JassVariable_JassVariableID");
            DropColumn("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            DropColumn("dbo.JassDerivers", "X2JassVariableID");
            DropColumn("dbo.JassDerivers", "X1JassVariableID");
            RenameColumn(table: "dbo.JassDerivers", name: "JassVariable_JassVariableID", newName: "JassVariableID");
            CreateIndex("dbo.JassDerivers", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "JassVariableID", "dbo.JassVariables", "JassVariableID", cascadeDelete: true);
        }
    }
}

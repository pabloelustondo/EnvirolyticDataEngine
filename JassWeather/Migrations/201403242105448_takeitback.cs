namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class takeitback : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassDerivers", "JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "JassFormula_JassVariableID", "dbo.JassVariables");
            DropIndex("dbo.JassDerivers", new[] { "JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "X1JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "X2JassVariable_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "JassFormula_JassVariableID" });
            DropTable("dbo.JassDerivers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.JassDerivers",
                c => new
                    {
                        JassDeriverID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        JassVariableID = c.Int(nullable: false),
                        X1JassVariableID = c.Int(),
                        X2JassVariableID = c.Int(),
                        JassFormulaID = c.Int(nullable: false),
                        YearStart = c.Int(nullable: false),
                        YearEnd = c.Int(nullable: false),
                        MonthStart = c.Int(nullable: false),
                        MnnthEnd = c.Int(nullable: false),
                        DayStart = c.Int(nullable: false),
                        DayEnd = c.Int(nullable: false),
                        JassVariable_JassVariableID = c.Int(),
                        X1JassVariable_JassVariableID = c.Int(),
                        X2JassVariable_JassVariableID = c.Int(),
                        JassFormula_JassVariableID = c.Int(),
                    })
                .PrimaryKey(t => t.JassDeriverID);
            
            CreateIndex("dbo.JassDerivers", "JassFormula_JassVariableID");
            CreateIndex("dbo.JassDerivers", "X2JassVariable_JassVariableID");
            CreateIndex("dbo.JassDerivers", "X1JassVariable_JassVariableID");
            CreateIndex("dbo.JassDerivers", "JassVariable_JassVariableID");
            AddForeignKey("dbo.JassDerivers", "JassFormula_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X2JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "X1JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "JassVariable_JassVariableID", "dbo.JassVariables", "JassVariableID");
        }
    }
}

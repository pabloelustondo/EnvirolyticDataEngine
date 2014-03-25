namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aaa1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassDerivers",
                c => new
                    {
                        JassDeriverID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        JassVariableID = c.Int(nullable: false),
                        JassFormulaID = c.Int(nullable: false),
                        YearStart = c.Int(nullable: false),
                        YearEnd = c.Int(nullable: false),
                        MonthStart = c.Int(nullable: false),
                        MnnthEnd = c.Int(nullable: false),
                        DayStart = c.Int(nullable: false),
                        DayEnd = c.Int(nullable: false),
                        JassVariable_JassVariableID = c.Int(),
                        JassFormula_JassVariableID = c.Int(),
                    })
                .PrimaryKey(t => t.JassDeriverID)
                .ForeignKey("dbo.JassVariables", t => t.JassVariable_JassVariableID)
                .ForeignKey("dbo.JassVariables", t => t.JassFormula_JassVariableID)
                .Index(t => t.JassVariable_JassVariableID)
                .Index(t => t.JassFormula_JassVariableID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassDerivers", new[] { "JassFormula_JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "JassVariable_JassVariableID" });
            DropForeignKey("dbo.JassDerivers", "JassFormula_JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "JassVariable_JassVariableID", "dbo.JassVariables");
            DropTable("dbo.JassDerivers");
        }
    }
}

namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addderiveragain : DbMigration
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
                        JassGridID = c.Int(nullable: false),
                        X1 = c.String(),
                        X2 = c.String(),
                        JassFormulaID = c.Int(nullable: false),
                        YearStart = c.Int(nullable: false),
                        YearEnd = c.Int(nullable: false),
                        MonthStart = c.Int(nullable: false),
                        MnnthEnd = c.Int(nullable: false),
                        DayStart = c.Int(nullable: false),
                        DayEnd = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JassDeriverID)
                .ForeignKey("dbo.JassVariables", t => t.JassVariableID, cascadeDelete: true)
                .ForeignKey("dbo.JassGrids", t => t.JassGridID, cascadeDelete: true)
                .ForeignKey("dbo.JassFormulas", t => t.JassFormulaID, cascadeDelete: true)
                .Index(t => t.JassVariableID)
                .Index(t => t.JassGridID)
                .Index(t => t.JassFormulaID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.JassDerivers", new[] { "JassFormulaID" });
            DropIndex("dbo.JassDerivers", new[] { "JassGridID" });
            DropIndex("dbo.JassDerivers", new[] { "JassVariableID" });
            DropForeignKey("dbo.JassDerivers", "JassFormulaID", "dbo.JassFormulas");
            DropForeignKey("dbo.JassDerivers", "JassGridID", "dbo.JassGrids");
            DropForeignKey("dbo.JassDerivers", "JassVariableID", "dbo.JassVariables");
            DropTable("dbo.JassDerivers");
        }
    }
}

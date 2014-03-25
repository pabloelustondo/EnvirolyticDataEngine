namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removederiveragain : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.JassDerivers", "JassVariableID", "dbo.JassVariables");
            DropForeignKey("dbo.JassDerivers", "JassFormulaID", "dbo.JassFormulas");
            DropIndex("dbo.JassDerivers", new[] { "JassVariableID" });
            DropIndex("dbo.JassDerivers", new[] { "JassFormulaID" });
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
                .PrimaryKey(t => t.JassDeriverID);
            
            CreateIndex("dbo.JassDerivers", "JassFormulaID");
            CreateIndex("dbo.JassDerivers", "JassVariableID");
            AddForeignKey("dbo.JassDerivers", "JassFormulaID", "dbo.JassFormulas", "JassFormulaID", cascadeDelete: true);
            AddForeignKey("dbo.JassDerivers", "JassVariableID", "dbo.JassVariables", "JassVariableID", cascadeDelete: true);
        }
    }
}

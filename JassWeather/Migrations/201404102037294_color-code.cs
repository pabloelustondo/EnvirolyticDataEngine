namespace JassWeather.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class colorcode : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JassColorCodes",
                c => new
                    {
                        JassColorCodeID = c.Int(nullable: false, identity: true),
                        Color0 = c.String(),
                        Value1 = c.Int(nullable: false),
                        Color1 = c.String(),
                        Value2 = c.Int(nullable: false),
                        Color2 = c.String(),
                        Value3 = c.Int(nullable: false),
                        Color3 = c.String(),
                        Value4 = c.Int(nullable: false),
                        Color4 = c.String(),
                        Value5 = c.Int(nullable: false),
                        Color5 = c.String(),
                        Value6 = c.Int(nullable: false),
                        Color6 = c.String(),
                        Value7 = c.Int(nullable: false),
                        Color7 = c.String(),
                        Value8 = c.Int(nullable: false),
                        Color8 = c.String(),
                        Value9 = c.Int(nullable: false),
                        Color9 = c.String(),
                        Value10 = c.Int(nullable: false),
                        Color10 = c.String(),
                    })
                .PrimaryKey(t => t.JassColorCodeID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JassColorCodes");
        }
    }
}

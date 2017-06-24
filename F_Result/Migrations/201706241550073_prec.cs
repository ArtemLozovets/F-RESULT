namespace F_Result.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class prec : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlanCredit",
                c => new
                    {
                        PlanCreditId = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Sum = c.Decimal(nullable: false, precision: 12, scale: 2),
                        ProjectId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Appointment = c.String(maxLength: 200),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.PlanCreditId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PlanDebit",
                c => new
                    {
                        PlanDebitId = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Sum = c.Decimal(nullable: false, precision: 12, scale: 2),
                        ProjectId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Appointment = c.String(maxLength: 200),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.PlanDebitId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            AlterColumn("dbo.ActualDebit", "Sum", c => c.Decimal(nullable: false, precision: 12, scale: 2));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlanDebit", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlanCredit", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.PlanDebit", new[] { "UserId" });
            DropIndex("dbo.PlanCredit", new[] { "UserId" });
            AlterColumn("dbo.ActualDebit", "Sum", c => c.Decimal(nullable: false, precision: 10, scale: 2));
            DropTable("dbo.PlanDebit");
            DropTable("dbo.PlanCredit");
        }
    }
}

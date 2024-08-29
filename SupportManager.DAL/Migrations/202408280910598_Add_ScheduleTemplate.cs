namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Add_ScheduleTemplate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ScheduleTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamId = c.Int(nullable: false),
                        Name = c.String(),
                        StartDay = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SupportTeams", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.TeamId);

            CreateTable(
                "dbo.ScheduleTemplateEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TemplateId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        Time = c.Time(nullable: false, precision: 7),
                        UserSlot = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTemplates", t => t.TemplateId, cascadeDelete: true)
                .Index(t => t.TemplateId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.ScheduleTemplates", "TeamId", "dbo.SupportTeams");
            DropForeignKey("dbo.ScheduleTemplateEntries", "TemplateId", "dbo.ScheduleTemplates");
            DropIndex("dbo.ScheduleTemplateEntries", new[] { "TemplateId" });
            DropIndex("dbo.ScheduleTemplates", new[] { "TeamId" });
            DropTable("dbo.ScheduleTemplateEntries");
            DropTable("dbo.ScheduleTemplates");
        }
    }
}

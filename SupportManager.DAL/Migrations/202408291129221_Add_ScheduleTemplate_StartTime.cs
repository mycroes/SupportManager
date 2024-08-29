namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ScheduleTemplate_StartTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleTemplates", "StartTime", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScheduleTemplates", "StartTime");
        }
    }
}

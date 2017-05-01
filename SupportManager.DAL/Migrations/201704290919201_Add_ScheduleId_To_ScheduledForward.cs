namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ScheduleId_To_ScheduledForward : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduledForwards", "ScheduleId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScheduledForwards", "ScheduleId");
        }
    }
}

namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_PhoneNumberId_To_ScheduledForward : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ScheduledForwards", "PhoneNumber_Id", "dbo.UserPhoneNumbers");
            DropIndex("dbo.ScheduledForwards", new[] { "PhoneNumber_Id" });
            RenameColumn(table: "dbo.ScheduledForwards", name: "PhoneNumber_Id", newName: "PhoneNumberId");
            AlterColumn("dbo.ScheduledForwards", "PhoneNumberId", c => c.Int(nullable: false));
            CreateIndex("dbo.ScheduledForwards", "PhoneNumberId");
            AddForeignKey("dbo.ScheduledForwards", "PhoneNumberId", "dbo.UserPhoneNumbers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ScheduledForwards", "PhoneNumberId", "dbo.UserPhoneNumbers");
            DropIndex("dbo.ScheduledForwards", new[] { "PhoneNumberId" });
            AlterColumn("dbo.ScheduledForwards", "PhoneNumberId", c => c.Int());
            RenameColumn(table: "dbo.ScheduledForwards", name: "PhoneNumberId", newName: "PhoneNumber_Id");
            CreateIndex("dbo.ScheduledForwards", "PhoneNumber_Id");
            AddForeignKey("dbo.ScheduledForwards", "PhoneNumber_Id", "dbo.UserPhoneNumbers", "Id");
        }
    }
}

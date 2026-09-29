namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_User_IsObserver : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsObserver", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsObserver");
        }
    }
}

namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Add_User_IsSuperUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsSuperUser", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Users", "IsSuperUser");
        }
    }
}

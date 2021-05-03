namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Rename_ComPort_To_ConnectionString : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.SupportTeams", "ComPort", "ConnectionString");
        }

        public override void Down()
        {
            RenameColumn("dbo.SupportTeams", "ConnectionString", "ComPort");
        }
    }
}

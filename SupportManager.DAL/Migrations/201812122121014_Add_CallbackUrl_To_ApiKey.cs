namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_CallbackUrl_To_ApiKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApiKeys", "CallbackUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApiKeys", "CallbackUrl");
        }
    }
}

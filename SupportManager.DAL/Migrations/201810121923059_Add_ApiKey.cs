namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ApiKey : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApiKeys",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Value = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApiKeys", "UserId", "dbo.Users");
            DropIndex("dbo.ApiKeys", new[] { "UserId" });
            DropTable("dbo.ApiKeys");
        }
    }
}

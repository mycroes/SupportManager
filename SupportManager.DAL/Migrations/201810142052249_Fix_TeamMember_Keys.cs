namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix_TeamMember_Keys : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.TeamMembers", name: "Team_Id", newName: "TeamId");
            RenameColumn(table: "dbo.TeamMembers", name: "User_Id", newName: "UserId");
            RenameIndex(table: "dbo.TeamMembers", name: "IX_Team_Id", newName: "IX_TeamId");
            RenameIndex(table: "dbo.TeamMembers", name: "IX_User_Id", newName: "IX_UserId");
            DropPrimaryKey("dbo.TeamMembers");
            AddPrimaryKey("dbo.TeamMembers", new[] { "TeamId", "UserId" });
            DropColumn("dbo.TeamMembers", "Id");
            DropColumn("dbo.TeamMembers", "Deleted");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TeamMembers", "Deleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.TeamMembers", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.TeamMembers");
            AddPrimaryKey("dbo.TeamMembers", "Id");
            RenameIndex(table: "dbo.TeamMembers", name: "IX_UserId", newName: "IX_User_Id");
            RenameIndex(table: "dbo.TeamMembers", name: "IX_TeamId", newName: "IX_Team_Id");
            RenameColumn(table: "dbo.TeamMembers", name: "UserId", newName: "User_Id");
            RenameColumn(table: "dbo.TeamMembers", name: "TeamId", newName: "Team_Id");
        }
    }
}

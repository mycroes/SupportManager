namespace SupportManager.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ForwardingStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamId = c.Int(nullable: false),
                        When = c.DateTimeOffset(nullable: false, precision: 7),
                        DetectedPhoneNumberId = c.Int(),
                        RawPhoneNumber = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserPhoneNumbers", t => t.DetectedPhoneNumberId)
                .ForeignKey("dbo.SupportTeams", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.TeamId)
                .Index(t => t.DetectedPhoneNumberId);
            
            CreateTable(
                "dbo.UserPhoneNumbers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Label = c.String(nullable: false),
                        Value = c.String(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                        VerificationToken = c.String(),
                        UserId = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(),
                        Login = c.String(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PrimaryEmailAddress_Id = c.Int(),
                        PrimaryPhoneNumber_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserEmailAddresses", t => t.PrimaryEmailAddress_Id)
                .ForeignKey("dbo.UserPhoneNumbers", t => t.PrimaryPhoneNumber_Id)
                .Index(t => t.PrimaryEmailAddress_Id)
                .Index(t => t.PrimaryPhoneNumber_Id);
            
            CreateTable(
                "dbo.UserEmailAddresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                        VerificationToken = c.String(),
                        UserId = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.SupportTeams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ComPort = c.String(),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TeamMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsAdministrator = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        Team_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SupportTeams", t => t.Team_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Team_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ScheduledForwards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamId = c.Int(nullable: false),
                        When = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        PhoneNumber_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserPhoneNumbers", t => t.PhoneNumber_Id)
                .ForeignKey("dbo.SupportTeams", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.TeamId)
                .Index(t => t.PhoneNumber_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ScheduledForwards", "TeamId", "dbo.SupportTeams");
            DropForeignKey("dbo.ScheduledForwards", "PhoneNumber_Id", "dbo.UserPhoneNumbers");
            DropForeignKey("dbo.ForwardingStates", "TeamId", "dbo.SupportTeams");
            DropForeignKey("dbo.TeamMembers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.TeamMembers", "Team_Id", "dbo.SupportTeams");
            DropForeignKey("dbo.ForwardingStates", "DetectedPhoneNumberId", "dbo.UserPhoneNumbers");
            DropForeignKey("dbo.Users", "PrimaryPhoneNumber_Id", "dbo.UserPhoneNumbers");
            DropForeignKey("dbo.Users", "PrimaryEmailAddress_Id", "dbo.UserEmailAddresses");
            DropForeignKey("dbo.UserPhoneNumbers", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserEmailAddresses", "UserId", "dbo.Users");
            DropIndex("dbo.ScheduledForwards", new[] { "PhoneNumber_Id" });
            DropIndex("dbo.ScheduledForwards", new[] { "TeamId" });
            DropIndex("dbo.TeamMembers", new[] { "User_Id" });
            DropIndex("dbo.TeamMembers", new[] { "Team_Id" });
            DropIndex("dbo.UserEmailAddresses", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "PrimaryPhoneNumber_Id" });
            DropIndex("dbo.Users", new[] { "PrimaryEmailAddress_Id" });
            DropIndex("dbo.UserPhoneNumbers", new[] { "UserId" });
            DropIndex("dbo.ForwardingStates", new[] { "DetectedPhoneNumberId" });
            DropIndex("dbo.ForwardingStates", new[] { "TeamId" });
            DropTable("dbo.ScheduledForwards");
            DropTable("dbo.TeamMembers");
            DropTable("dbo.SupportTeams");
            DropTable("dbo.UserEmailAddresses");
            DropTable("dbo.Users");
            DropTable("dbo.UserPhoneNumbers");
            DropTable("dbo.ForwardingStates");
        }
    }
}

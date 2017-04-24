namespace Texxtoor.DataModels.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hh : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Common.AddressBook", "Profile_Id", "Common.UserProfiles");
            AddColumn("Common.UserProfiles", "AvailabilityContainer", c => c.String());
            AddForeignKey("Common.AddressBook", "Profile_Id", "Common.UserProfiles", "Id", cascadeDelete: true);
            DropColumn("Common.UserProfiles", "StartAvailability");
            DropColumn("Common.UserProfiles", "EndAvailability");
        }
        
        public override void Down()
        {
            AddColumn("Common.UserProfiles", "EndAvailability", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("Common.UserProfiles", "StartAvailability", c => c.DateTime(precision: 7, storeType: "datetime2"));
            DropForeignKey("Common.AddressBook", "Profile_Id", "Common.UserProfiles");
            DropColumn("Common.UserProfiles", "AvailabilityContainer");
            AddForeignKey("Common.AddressBook", "Profile_Id", "Common.UserProfiles", "Id");
        }
    }
}

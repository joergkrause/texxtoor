namespace Texxtoor.DataModels.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class avail : DbMigration
    {
        public override void Up()
        {
            AddColumn("Common.UserProfiles", "AvailabilityContainer", c => c.String());
            DropColumn("Common.UserProfiles", "StartAvailability");
            DropColumn("Common.UserProfiles", "EndAvailability");
        }
        
        public override void Down()
        {
            AddColumn("Common.UserProfiles", "EndAvailability", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("Common.UserProfiles", "StartAvailability", c => c.DateTime(precision: 7, storeType: "datetime2"));
            DropColumn("Common.UserProfiles", "AvailabilityContainer");
        }
    }
}

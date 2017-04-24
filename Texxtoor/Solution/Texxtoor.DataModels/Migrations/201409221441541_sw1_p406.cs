namespace Texxtoor.DataModels.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sw1_p406 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Common.Country", "MarketingPackage_Id", "Marketing.Package");
            DropForeignKey("Common.AddressBook", "Country_Id", "Common.Country");
            DropIndex("Common.Country", new[] { "MarketingPackage_Id" });
            DropIndex("Common.AddressBook", new[] { "Country_Id" });
            AddColumn("Marketing.Package", "LimitCountriesContainer", c => c.String());
            AddColumn("Common.AddressBook", "Country", c => c.String());
            DropColumn("Common.Country", "MarketingPackage_Id");
            DropColumn("Common.AddressBook", "Country_Id");
        }
        
        public override void Down()
        {
            AddColumn("Common.AddressBook", "Country_Id", c => c.Int());
            AddColumn("Common.Country", "MarketingPackage_Id", c => c.Int());
            DropColumn("Common.AddressBook", "Country");
            DropColumn("Marketing.Package", "LimitCountriesContainer");
            CreateIndex("Common.AddressBook", "Country_Id");
            CreateIndex("Common.Country", "MarketingPackage_Id");
            AddForeignKey("Common.AddressBook", "Country_Id", "Common.Country", "Id");
            AddForeignKey("Common.Country", "MarketingPackage_Id", "Marketing.Package", "Id");
        }
    }
}

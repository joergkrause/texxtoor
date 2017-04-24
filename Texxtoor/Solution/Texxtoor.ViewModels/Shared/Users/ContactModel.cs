using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Texxtoor.DataModels;

namespace Texxtoor.ViewModels.Users {

  public class ContactModel {

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ContactModel_Name_The_Name_is_mandatory")]
    [Display(ResourceType = typeof(ModelResources), Name = "ContactModel_Name_Your_name", Description="ContactModel_Name_Your_name_Helptext")]
    [StringLength(256)]
    public string Name { get; set; }

    [EmailAddress(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "ContactModel_EMail_The_e_mail_is_not_properly_formatted", ErrorMessage = null)]
    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ContactModel_EMail_The_e_mail_is_mandatory")]
    [Display(ResourceType = typeof(ModelResources), Name = "ContactModel_EMail_Your_e_mail_for_response", Description="ContactModel_EMail_Your_e_mail_for_response_Helptext")]
    [StringLength(256)]
    [DataType(DataType.EmailAddress)]
    
    public string EMail { get; set; }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ContactModel_Subject_The_Subject_is_mandatory")]
    [Display(ResourceType = typeof(ModelResources), Name = "ContactModel_Subject_Subject", Description="ContactModel_Subject_Subject_Helptext")]
    [StringLength(512)]
    public string Subject { get; set; }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ContactModel_Message_Consider_writing_us_what_s_the_purpose_of_you_contact_")]
    [Display(ResourceType = typeof(ModelResources), Name = "ContactModel_Message_Your_message", Description="ContactModel_Message_Your_message_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 5)]
    [AdditionalMetadata("Cols", 55)]
    public string Message { get; set; }
    

  }
}
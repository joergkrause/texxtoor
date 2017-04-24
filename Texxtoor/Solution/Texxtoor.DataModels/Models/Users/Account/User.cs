using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;

namespace Texxtoor.DataModels.Models.Users {

  [DebuggerDisplay("User [{UserName}]")]
  [Table("Users", Schema = "Common")]
  public class User : IdentityUser<int, TexxtoorLogin, TexxtoorUserRole, TexxtoorUserClaim> {

    public User() {
      CreateDate = DateTime.Now;
      LastLoginDate = DateTime.MinValue;
      LastPasswordChangedDate = DateTime.MinValue;
      LastLockoutDate = DateTime.MinValue;
      FailedPasswordAttemptWindowStart = DateTime.Now;
      FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
      CreatedAt = DateTime.Now;
      ModifiedAt = DateTime.Now;
    }

    # region EntityBase

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ScaffoldColumn(false)]
    [SuppressPropertyCopy]
    public override int Id { get; set; }

    [ScaffoldColumn(false)]
    [Column(TypeName = "datetime2")]
    [SuppressPropertyCopy]
    [Display(ResourceType = typeof(ModelResources), Name = "EntityBase_CreatedAt_Created_At")]
    public DateTime CreatedAt { get; set; }

    [ScaffoldColumn(false)]
    [Column(TypeName = "datetime2")]
    [SuppressPropertyCopy]
    [Display(ResourceType = typeof(ModelResources), Name = "EntityBase_ModifiedAt_Modified_At")]
    public DateTime ModifiedAt { get; set; }

    # endregion


    [Display(ResourceType = typeof(ModelResources), Name = "User_Application_Applications", Description="User_Application_Applications_Helptext")]
    public virtual Application Application { get; set; }

    //[Required]
    [StringLength(256), Display(Name = "User_Application_Password", ResourceType = typeof(ModelResources), Description = "User_Password_User_s_password_")]
    [DataType(DataType.Password)]
    [ScaffoldColumn(false)]
    public string Password { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_PasswordFormat_Password_Format", Description="User_PasswordFormat_Password_Format_Helptext")]
    [UIHint("PasswordFormat")]
    public MembershipPasswordFormat PasswordFormat { get; set; }

    /// <summary>
    /// Use this to manage the activation mail sequence.
    /// </summary>
    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_PasswordSalt_Password_Salt", Description="User_PasswordSalt_Password_Salt_Helptext")]
    [ScaffoldColumn(false)]
    public string PasswordSalt { get; set; }

    [StringLength(16)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_MobilePin_Mobile_PIN", Description="User_MobilePin_Mobile_PIN_Helptext")]
    [ScaffoldColumn(false)]
    public string MobilePin { get; set; }

    [EmailAddress]
    [StringLength(256)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_Email_Email", Description="User_Email_Email_Helptext")]
    public override string Email {
      get { return base.Email; }
      set { base.Email = value; }
    }

    [NotMapped]
    [StringLength(256)]
    [ScaffoldColumn(false)]
    public string LoweredEmail {
      get { return Email == null ? "" : Email.ToLowerInvariant(); }
    }

    [StringLength(256)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_PasswordQuestion_Password_Question", Description="User_PasswordQuestion_Password_Question_Helptext")]
    public string PasswordQuestion { get; set; }

    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_PasswordAnswer_Question_answer", Description="User_PasswordAnswer_Question_answer_Helptext")]
    public string PasswordAnswer { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_IsApproved_Is_Approved", Description="User_IsApproved_Is_Approved_Helptext")]
    public bool IsApproved { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_IsLockedOut_Is_Locked_Out", Description="User_IsLockedOut_Is_Locked_Out_Helptext")]
    public bool IsLockedOut { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "User_CreateDate_Create_Date", Description="User_CreateDate_Create_Date_Helptext")]
    public DateTime CreateDate { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "User_LastLoginDate_Last_Login_Date", Description="User_LastLoginDate_Last_Login_Date_Helptext")]
    [ScaffoldColumn(false)]
    public DateTime LastLoginDate { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "User_LastPasswordChangedDate_Last_Password_Change", Description="User_LastPasswordChangedDate_Last_Password_Change_Helptext")]
    [ScaffoldColumn(false)]
    public DateTime LastPasswordChangedDate { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "User_LastLockoutDate_Last_Lockout", Description="User_LastLockoutDate_Last_Lockout_Helptext")]
    [ScaffoldColumn(false)]
    public DateTime LastLockoutDate { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_FailedPasswordAttemptCount_Failed_password_attempts", Description="User_FailedPasswordAttemptCount_Failed_password_attempts_Helptext")]
    [ScaffoldColumn(false)]
    public int FailedPasswordAttemptCount { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "User_FailedPasswordAttemptWindowStart_Failed_password_attempt_window", Description="User_FailedPasswordAttemptWindowStart_Failed_password_attempt_window_Helptext")]
    [ScaffoldColumn(false)]
    public DateTime FailedPasswordAttemptWindowStart { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_FailedPasswordAnswerAttemptCount_Failed_password_answer_attempt_counts", Description="User_FailedPasswordAnswerAttemptCount_Failed_password_answer_attempt_counts_Helptext")]
    [ScaffoldColumn(false)]
    public int FailedPasswordAnswerAttemptCount { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "User_FailedPasswordAnswerAttemptWindowStart_Failed_password_anser_attempt_window_start", Description="User_FailedPasswordAnswerAttemptWindowStart_Failed_password_anser_attempt_window_start_Helptext")]
    [ScaffoldColumn(false)]
    public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_Comment_Comment", Description="User_Comment_Comment_Helptext")]
    public string Comment { get; set; }

    # region --= User Specific Properties =--

    [Required]
    [StringLength(256)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_UserName_User_Name", Description = "User_UserName_User_Name_Helptext")]
    public override string UserName { get; set; }

    [NotMapped]
    [StringLength(256)]
    [ScaffoldColumn(false)]
    public string LoweredUserName {
      get { return UserName == null ? "" : UserName.ToLowerInvariant(); }
    }

    [StringLength(16)]
    [Display(ResourceType = typeof(ModelResources), Name = "User_MobileAlias_Mobile_Alias", Description = "User_MobileAlias_Mobile_Alias_Helptext")]
    public string MobileAlias { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_IsAnonymous_Is_Anonymous", Description = "User_IsAnonymous_Is_Anonymous_Helptext")]
    public bool IsAnonymous { get; set; }

    [ScaffoldColumn(false)]
    [Column(TypeName = "datetime2")]
    public DateTime LastActivityDate { get; set; }

    /// <summary>
    /// The id returned by the OAuth provider to identify the local account.
    /// </summary>
    [StringLength(50)]
    [ScaffoldColumn(false)]
    public string OAuthUserId { get; set; }

    /// <summary>
    /// The name of the external provider, such as "facebook", "google", or whatever
    /// </summary>
    [StringLength(50)]
    [ScaffoldColumn(false)]
    public string OAuthProvider { get; set; }

    
    [ScaffoldColumn(false)]
    public int? LeadingAccountId { get; set; }

    /// <summary>
    /// Use this to manage the activation mail sequence.
    /// </summary>
    [StringLength(128)]
    [ScaffoldColumn(false)]
    public string LinkSalt { get; set; }

    # endregion --= User Specific Properties =--

    # region --= User Specific Relations =--

    //[Display(ResourceType = typeof(ModelResources), Name = "User_Roles_User_s_Roles", Description="User_Roles_User_s_Roles_Helptext")]
    //[UIHint("Roles")]
    //public override IList<Role> Roles { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_ReaderGroupsMember_Reading_groups", Description="User_ReaderGroupsMember_Reading_groups_Helptext")]
    public IList<ReaderGroup> ReaderGroupsMember { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "User_ReaderGroupsAdmin_Reading_groups_where_user_is_admin", Description="User_ReaderGroupsAdmin_Reading_groups_where_user_is_admin_Helptext")]
    public IList<ReaderGroup> ReaderGroupsAdmin{ get; set; }

    [ScaffoldColumn(false)]
    public IList<Element> Elements { get; set; }

    public IList<Published> Published { get; set; }

    public IList<TeamMember> Teams { get; set; }

    public IList<ReaderGroup> Groups { get; set; }

    public IList<UserFile> PrivateFiles { get; set; }

    public IList<UserActivity> UserActivities { get; set; }

    public virtual UserProfile Profile { get; set; }

    public virtual IList<TermSet> TermSets { get; set; }

    public IList<TemplateGroup> TemplateGroups { get; set; }

    /// <summary>
    /// The tenant the user is related to. Public users have no relation, hence it can be null.
    /// </summary>
    public Tenant Tenant { get; set; }  

    # endregion --= User Specific Relations =--

    # region

    [ScaffoldColumn(false)]
    public override ICollection<TexxtoorUserClaim> Claims {
      get {
        return base.Claims;
      }
    }

    [ScaffoldColumn(false)]
    public override ICollection<TexxtoorLogin> Logins {
      get {
        return base.Logins;
      }
    }

    [ScaffoldColumn(false)]
    public override bool LockoutEnabled {
      get {
        return base.LockoutEnabled;
      }
      set {
        base.LockoutEnabled = value;
      }
    }

    # endregion

  }

}
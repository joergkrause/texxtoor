using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Users {

  /// <summary>
  /// Profile Information for the reader, used to match reader with opuses.
  /// </summary>
  [Table("UserProfiles", Schema = "Common")]
  public class UserProfile : EntityBase {

    public UserProfile() {
      ConsumerMatrix = new List<ConsumerMatrix>(); // WebGrid requires NOT NULL
      ContributorMatrix = new List<ContributorMatrix>();
      LanguageMatrix = new List<LanguageMatrix>();
      // ReSharper disable once DoNotCallOverridableMethodsInConstructor
      RunControl = new RunControl {
        UiLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName,
        Complexity = Complexity.Simple,
        RunMode = RunMode.Texxtoor
      };
      AwardState = 1;
    }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_User_User", Description = "UserProfile_User_User_Description_Helptext")]
    public virtual User User { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Description_Description", Description = "UserProfile_Description_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 45)]
    public string Description { get; set; }

    [AllowHtml]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Walltext_Wall_Text", Description = "UserProfile_Walltext_Wall_Text_Helptext")]
    public string Walltext { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Notes_Public_Notes", Description = "UserProfile_Notes_Public_Notes_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 2)]
    [AdditionalMetadata("Cols", 45)]
    public string Notes { get; set; }

    public AccountStatus Status { get; set; }

    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_FirstName_FirstName", Description = "UserProfile_FirstName_FirstName_Helptext")]
    public string FirstName { get; set; }

    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_LastName_LastName", Description = "UserProfile_LastName_LastName_Helptext")]
    public string LastName { get; set; }

    [StringLength(64)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_MiddleName_MiddleName", Description = "UserProfile_MiddleName_MiddleName_Helptext")]
    public string MiddleName { get; set; }

    [StringLength(2048)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_StatementOfTaste_StatementOfTaste", Description = "UserProfile_StatementOfTaste_StatementOfTaste_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 2)]
    [AdditionalMetadata("Cols", 45)]
    public string StatementOfTaste { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Newsletter_Newsletter", Description = "UserProfile_Newsletter_Newsletter_Helptext")]
    public bool? Newsletter { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Gender_Gender", Description = "UserProfile_Gender_Gender_Helptext")]
    public Gender? Gender { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_BirthDay_BirthDay", Description = "UserProfile_BirthDay_BirthDay_Helptext")]
    [UIHint("Birthday")]
    public DateTime? BirthDay { get; set; }

    /// <summary>
    /// Used to provide values for consuming content, mostly used by readers.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_ConsumerMatrix_Consumer_Matrix", Description = "UserProfile_ConsumerMatrix_Consumer_Matrix_Helptext")]
    public List<ConsumerMatrix> ConsumerMatrix { get; set; }

    /// <summary>
    /// Provides values for contributors, mostly used by authors and contributors.
    /// </summary>
    /// <remarks>Contributors for multiple languages may apply multiple times (copyeditor lang="es" is different from copeditor lang="en")</remarks>
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_ContributorMatrix_Contributor_Skill_Matrix", Description = "UserProfile_ContributorMatrix_Contributor_Skill_Matrix_Helptext")]
    public List<ContributorMatrix> ContributorMatrix { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_LanguageMatrix_Language_Matrix", Description = "UserProfile_LanguageMatrix_Language_Matrix_Helptext")]
    public List<LanguageMatrix> LanguageMatrix { get; set; }

    /// <summary>
    /// Supports web forms
    /// </summary>
    [NotMapped]
    public SelectList LanguageMatrixList {
      get {
        var sl = new SelectList(LanguageMatrix.Select(l => new {
          Value = l.LocaleId,
          Text = CultureInfo.GetCultureInfo(l.LocaleId).NativeName
        }), "Value", "Text");
        return sl;
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_XingProfile_Xing_Profile", Description = "UserProfile_XingProfile_Xing_Profile_Helptext")]
    [Url]
    public string XingProfile { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_FacebookProfile_Facebook_Profile", Description = "UserProfile_FacebookProfile_Facebook_Profile_Helptext")]
    [Url]
    public string FacebookProfile { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_OtherProfile_Other_Profile", Description = "UserProfile_OtherProfile_Other_Profile_Helptext")]
    [Url]
    public string OtherProfile { get; set; }

    public IList<Published> Favorites { get; set; }

    public byte[] Image { get; set; }

    public IList<AddressBook> Addresses { get; set; }

    // This is to let users rate others after contract work is done
    [ScaffoldColumn(false)]
    public IList<UserRating> Rating { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public decimal GlobalRating {
      get {
        return Rating != null && Rating.Any() ? Rating.Average(r => r.GetAverage()) : 0M;
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_AwardState_Award", Description = "UserProfile_AwardState_Award_Helptext")]
    public int AwardState { get; set; }

    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_UserScore_UserScore", Description = "UserProfile_UserScore_UserScore_Helptext")]
    public int UserScore {
      get {
        var context = UnitOfWorkFactory.GetIUnitOfWorkContext<PortalContext>();
        var score = AwardState;
        var user = System.Web.HttpContext.Current.User.Identity.Name;
        score += context.Works.Count(q => q.Owner.UserName == user) * 5;
        score += context.Bookmarks.Count(q => q.Owner.UserName == user) * 2;
        score += context.Comments.Count(q => q.Owner.UserName == user) * 3;
        score += context.ReaderGroups.Count(q => q.Owner.UserName == user) * 5;
        score += context.QuestionsAnswers.Count(q => q.Owner.UserName == user) * 10;
        score += context.OrderProducts.Count(o => o.Owner.UserName == user) * 5;
        return score;
      }
    }

    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_CreatorScore_CreatorScore", Description = "UserProfile_CreatorScore_CreatorScore_Helptext")]
    public int CreatorScore {
      get {
        var context = UnitOfWorkFactory.GetIUnitOfWorkContext<PortalContext>();
        var score = AwardState;
        var user = System.Web.HttpContext.Current.User.Identity.Name;
        score += context.Published.Count(p => p.Owner.UserName == user) * 200;
        score += (int)GlobalRating * 10;
        score += context.QuestionsAnswers.Count(q => q.Owner.UserName == user) * 10;
        score += context.TeamMembers.Count(t => t.Member.UserName == user && !t.TeamLead) * 20;
        score += context.TeamMembers.Count(t => t.Member.UserName == user && t.TeamLead) * 50;
        score += context.OrderProducts.Count(o => o.Work.Published != null && o.Work.Published.Owner.UserName == user) * 2;
        return score;
      }
    }

    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_UserBadge_UserBadge", Description = "UserProfile_UserBadge_UserBadge_Helptext")]
    public UserBadge UserBadge {
      get {
        var score = UserScore;
        if (score > 100) return UserBadge.Member;
        if (score > 1000) return UserBadge.Silver;
        if (score > 10000) return UserBadge.Gold;
        return score > 100000 ? UserBadge.Platinum : UserBadge.Rookie;
      }
    }

    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_CreatorBadge_CreatorBadge", Description = "UserProfile_CreatorBadge_CreatorBadge_Helptext")]
    public CreatorBadge CreatorBadge {
      get {
        var score = CreatorScore;
        if (score > 200) return CreatorBadge.Author;
        if (score > 2000) return CreatorBadge.Maven;
        if (score > 5000) return CreatorBadge.Specialist;
        return score > 10000 ? CreatorBadge.Expert : CreatorBadge.Beginner;
      }
    }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Application_Application", Description = "UserProfile_Application_Application_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 45)]
    public string Application { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_ShowWalltextOnApplication_Show_private_page", Description = "UserProfile_ShowWalltextOnApplication_Show_private_page_Helptext")]
    public bool? ShowWalltextOnApplication { get; set; }

    [ScaffoldColumn(false)]
    public string AvailabilityContainer { get; set; }

    private static string SerializeObject(object value) {
      return Convert.ToBase64String(StorageSerializer.Serialize(value));
    }

    private static object DeSerializeObject(string value) {
      return StorageSerializer.Deserialize(Convert.FromBase64String(value));
    }

    private ObservableCollection<AvailabilityFrame> _observableFrame;

    [NotMapped]
    public ObservableCollection<AvailabilityFrame> Availabilities {
      get {
        if (!String.IsNullOrEmpty(AvailabilityContainer)) {
          _observableFrame = DeSerializeObject(AvailabilityContainer) as ObservableCollection<AvailabilityFrame>;
        } else {
          _observableFrame = new ObservableCollection<AvailabilityFrame>();
        }
        _observableFrame.CollectionChanged += _observableFrame_CollectionChanged;
        return _observableFrame;
      }
    }

    void _observableFrame_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      AvailabilityContainer = SerializeObject(_observableFrame);
    }

    [NotMapped]
    public IList<AvailabilityFrame> FutureAvailabilities {
      get {
        var refDate = DateTime.Now;
        return Availabilities
          .Where(a => (a.StartAvailability > refDate) && (a.EndAvailability > refDate))
          .ToList();
      }
    }

    public class AvailabilityFrame {

      public AvailabilityFrame() {
        StartAvailability = DateTime.Today;
        EndAvailability = DateTime.Today.AddDays(5);
      }

      public AvailabilityFrame(DateTime start, DateTime end) {
        if (start > end) {
          throw new ArgumentOutOfRangeException("end data must be after start date");
        }
        StartAvailability = start;
        EndAvailability = end;
      }

      [Display(Name = "Kind")]
      public AvailabilityKind Kind { get; set; }

      public string GetKindLocalized() {
        return typeof(AvailabilityKind).GetField(Kind.ToString()).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().Single().GetName();
      }

      [NotMapped]
      [DoNotSerialize]
      [ScaffoldColumn(false)]
      public string GetKindLocalizedForGrouping {
        get { return GetKindLocalized(); } // supports grouping in gantt charts
      }

      [StringLength(50)]
      [Display(Name = "Name")]
      [Required]
      [Watermark("Headline")]
      public string Name { get; set; }

      [Column(TypeName = "datetime2")]
      [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_StartAvailability_Available_from", Description = "UserProfile_StartAvailability_Available_from_Helptext")]
      [UIHint("FutureDate")]
      public DateTime StartAvailability { get; set; }

      [Column(TypeName = "datetime2")]
      [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_EndAvailability_Available_until", Description = "UserProfile_EndAvailability_Available_until_Helptext")]
      [UIHint("FutureDate")]
      public DateTime EndAvailability { get; set; }

    }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_Hidden_Hidden", Description = "UserProfile_Hidden_Hidden_Helptext")]
    public bool? Hidden { get; set; }

    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_ExternalProfileUrl_External_Profil", Description = "UserProfile_ExternalProfileUrl_External_Profil_Helptext")]
    [Url]
    public string ExternalProfileUrl { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_ContractAccepted_Contract", Description = "UserProfile_ContractAccepted_Contract_Helptext")]
    public bool? ContractAccepted { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_SharingAccepted_Shared_Revenues", Description = "UserProfile_SharingAccepted_Shared_Revenues_Helptext")]
    public bool? SharingAccepted { get; set; }

    [Range(0, 500)]
    [UIHint("Decimal_Null")]
    [DataType(DataType.Currency)]
    [AdditionalMetadata("Length", "6")]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_MinHourlyRate_Hourly_min", Description = "UserProfile_MinHourlyRate_Hourly_min_Helptext")]
    public decimal? MinHourlyRate { get; set; }

    [Range(0, 500)]
    [UIHint("Decimal_Null")]
    [DataType(DataType.Currency)]
    [AdditionalMetadata("Length", "6")]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_MaxHourlyRate_Hourly_max", Description = "UserProfile_MaxHourlyRate_Hourly_max_Helptext")]
    public decimal? MaxHourlyRate { get; set; }

    # region Payment

    [StringLength(200)]
    [Display(ResourceType = typeof(ModelResources), Name = "UserProfile_PayPalUserId_Paypal_User_Id", Description = "UserProfile_PayPalUserId_Paypal_User_Id_Helptext")]
    public string PayPalUserId { get; set; }

    # endregion

    public virtual RunControl RunControl { get; set; }

    [NotMapped]
    public string FullName {
      get {
        return String.Format("{0}{3}{1}{3}{4}{2}", FirstName, MiddleName, LastName, String.IsNullOrEmpty(MiddleName) ? "" : " ", String.IsNullOrEmpty(MiddleName) ? " " : "");
      }
    }

    public bool HasDefaultAddress() {
      return Addresses != null && (Addresses.Count() == 1 || Addresses.Any(a => a.Default));
    }

    [NotMapped]
    public string FullDefaultAddress {
      get {
        if (HasDefaultAddress()) {
          var def = Addresses.FirstOrDefault(a => a.Default) ?? Addresses.First();
          return String.Format("{0}, {1} {2}, {3}", def.StreetNumber, def.Zip, def.City, def.Country ?? "");
        }
        return "";
      }
    }
  }

}
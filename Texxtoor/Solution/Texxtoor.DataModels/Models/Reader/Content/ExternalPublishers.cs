using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.ViewModels.Content {

  public class ExternalPublisher : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(PropertyChangedEventArgs args) {
      if (PropertyChanged != null) {
        PropertyChanged(this, args);
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_ConfirmPublish_Confirm_Publishing_Globally", Order = 1)]
    [Required(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "ExternalPublisher_ConfirmPublish_You_must_confirm")]
    public bool ConfirmPublish { get; set; }

  }

  public class ExternalPublisherSettings : ExternalPublisher {

    private string _title;
    private string _authors;
    private string _description;
    private string _language;
    private Isbn _isbn;
    private bool _creativeCommon;
    private string _categories;
    private string _keywords;
    private bool _drmRequired;
    private bool _requestsPublishing;

    # region KDP

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleTitle_Title", Description = "ExternalPublisher_KindleTitle_Title_Helptext")]
    [Required]
    public string Title {
      get { return _title; }
      set {
        _title = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleTitle"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleDescription_Name", Description = "ExternalPublisher_KindleDescription_Name_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 45)]
    [Required]
    public string Description {
      get { return _description; }
      set {
        _description = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleDescription"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleAuthors_Authors", Description = "ExternalPublisher_KindleAuthors_Authors_Helptext")]
    [Required]
    [RegularExpression(@"^\d{1,7}(?:[,]\d{1,7})*$")]
    public string Authors {
      get { return _authors; }
      set {
        _authors = value ?? String.Empty;
        try {
          var split = _authors.Split(',');
          int n;
          if (split.Any(s => !Int32.TryParse(s, out n) || n == 0)) {
            return;
          }
        } catch {
          _authors = String.Empty;
        }
        OnPropertyChanged(new PropertyChangedEventArgs("KindleAuthors"));
      }
    }

    [NotMapped]
    [ScaffoldColumn(false)]
    public IEnumerable<int> AuthorIds {
      get {
        return !String.IsNullOrEmpty(Authors)
                 ? Authors
                     .Split(',')
                     .Select(Int32.Parse)
                 : new int[0];
      }
      set { Authors = String.Join(",", value); }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleLanguage_Language", Description = "ExternalPublisher_KindleLanguage_Language_Helptext")]
    [Required]
    public string KindleLanguage {
      get { return _language; }
      set {
        _language = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleLanguage"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleIsbn_ISBN", Description = "ExternalPublisher_KindleIsbn_ISBN_Helptext")]
    [UIHint("Isbn")]
    public Isbn Isbn {
      get { return _isbn; }
      set {
        _isbn = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleIsbn"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleCreativeCommon_Confirm_Rights", Description = "ExternalPublisher_KindleCreativeCommon_Confirm_Rights_Helptext")]
    public bool CreativeCommon {
      get { return _creativeCommon; }
      set {
        _creativeCommon = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleCreativeCommon"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleCategories_Categories", Description = "ExternalPublisher_KindleCategories_Categories_Helptext")]
    [Required]
    public string Categories {
      get { return _categories; }
      set {
        _categories = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleCategories"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleKeywords_Search_Keywords", Description = "ExternalPublisher_KindleKeywords_Search_Keywords_Helptext")]
    [Required]
    public string Keywords {
      get { return _keywords; }
      set {
        _keywords = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleKeywords"));
      }
    }

    [Display(ResourceType = typeof(ModelResources), Name = "ExternalPublisher_KindleDrmRequired_DRM_Required", Description = "ExternalPublisher_KindleDrmRequired_DRM_Required_Helptext")]
    public bool DrmRequired {
      get { return _drmRequired; }
      set {
        _drmRequired = value;
        OnPropertyChanged(new PropertyChangedEventArgs("KindleDrmRequired"));
      }
    }

    public bool RequestsPublishing {
      get { return _requestsPublishing; }
      set {
        _requestsPublishing = value;
        OnPropertyChanged(new PropertyChangedEventArgs("RequestsPublishing"));
      }
    }

    # endregion

  }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Cms {

  [Table("Pages", Schema = "Cms")]
  public class CmsPage : LocalizedEntityBase {
    public CmsPage() {
      Status = StatusCode.Draft;
      Menu = new List<CmsMenu>();
      MenuItem = new List<CmsMenuItem>();
    }

    // all menus the page ist related to
    public IList<CmsMenu> Menu { get; set; }
    
    // all menu items the page is related to
    public IList<CmsMenuItem> MenuItem { get; set; }

    [Display(Name = "Status", Description = "The current editing state.")]
    public StatusCode Status { get; set; }

    [StringLength(128)]
    [Display(Name = "Search Engine Title", Description = "SEO optimized title.")]
    public string SeoTitle { get; set; }

    [Required]
    [StringLength(128)]
    [Display(Name = "Page Title", Description = "The title that appears on the screen.")]
    public string PageTitle { get; set; }

    [StringLength(32)]
    [Display(Name = "Alias", Description = "A short alias of the title used to create the viewpath.")]
    public string Alias { get; set; }

    [Display(Name = "User", Description = "USer that create/modified the page.")]
    public User Author { get; set; }

    string _pageContent;

    [Display(Name = "Content", Description = "Content displayd on the page.")]
    public string PageContent {
      get {
        if (_pageContent == null) return String.Empty;
        return _pageContent.Replace("<![CDATA[", "").Replace("]]>", ""); }
      set {
        _pageContent = value;
      }
    }

    [Display(Name = "Meta Description", Description = "SEO optimized description.")]
    [StringLength(512)]
    public string MetaDescription { get; set; }

    [Display(Name = "Meta Keywords", Description = "SEO optimized keywords.")]
    [StringLength(512)]
    public string MetaKeywords { get; set; }

    public static string CreateAlias(string title) {
      Regex regex = new Regex("[a-zA-Z0-9]{3,}");
      Match m = regex.Match(title);
      StringBuilder parsedTitle = new StringBuilder();
      while (m.Success) {
        Group g = m.Groups[0];
        parsedTitle.Append(g.Value + " ");

        m = m.NextMatch();
      }

      return parsedTitle.ToString().ToLower().Trim().Replace(" ", "-");
    }

    public string GetUrl() {
      return this.Alias + "-" + Id + "/";
    }
  }

}


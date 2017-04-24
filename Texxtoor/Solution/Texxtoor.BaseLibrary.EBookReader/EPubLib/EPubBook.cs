using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Table("Book", Schema = "Epub")]
  public class EpubBook : EntityBase {

    /// <summary>
    /// META-INF container element
    /// </summary>
    public virtual Container ContainerData { get; set; }

    /// <summary>
    /// The package element is the root container of the Package Document and encapsulates Publication metadata and resource information.
    /// </summary>
    public virtual OpfPackage PackageData { get; set; }

    public virtual Navigation NavigationData { get; set; }
    
    /// <summary>
    /// A description that is not part of the EPUB standard.
    /// </summary>
    [StringLength(512)]
    public string CoverDescription { get; set; }

    /// <summary>
    /// A cover that is not part of the EPUB standard.
    /// </summary>
    public byte[] CoverImage { get; set; }

    [NotMapped]
    [ScaffoldColumn(false)]
    public string Author {
      get {
        return PackageData.MetaData.Creator.Text;
      }
    }


    # region Shortcuts to elements

    private List<NavPoint> _flatToc; // all in one collection (1, 1.1, 1.1.1, 1.1.2, 1.2, 2 etc.)
    
    public Navigation GetTableOfContent() {
      return NavigationData;
    }

    public List<NavPoint> GetFlatTableOfContent() {
      if (_flatToc == null) {
        _flatToc = new List<NavPoint>();
        FlattenNavHierarchy(_flatToc, NavigationData.NavMap, null);
      }
      return _flatToc;
    }

    // get complete navigation tree of the epub --> move to Ebook lib ??
    private void FlattenNavHierarchy(List<NavPoint> flatList, IEnumerable<NavPoint> navPoints, NavPoint parent) {
      foreach (var navPoint in navPoints) {
        navPoint.Parent = parent;
        flatList.Add(navPoint);
        if (navPoint.Children != null) {
          FlattenNavHierarchy(flatList, navPoint.Children, navPoint);
        }        
      }
    }

    public IEnumerable<ManifestItem> GetAllFiles() {
      return PackageData.Manifest.Items;
    }

    /// <summary>
    /// All CSS, images, videos and everything but content.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ManifestItem> GetResourceFiles() {
      return PackageData.Manifest.Items.Except(GetContentFiles());
    }

    /// <summary>
    /// All HTML content
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ContentFile> GetContentFiles() {
      return PackageData.Manifest.Items.OfType<ContentFile>();
    }

    /// <summary>
    /// All of type image.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ImageFile> GetImageFiles() {
      return PackageData.Manifest.Items.OfType<ImageFile>();
    }

    # endregion

    
    private string GetBodyContent(byte[] epub) {
      string text = ASCIIEncoding.ASCII.GetString(epub);
      text = text.Substring(text.IndexOf("<body>") + "<body>".Length);
      text = text.Substring(0, text.IndexOf("</body>"));
      return text;
    }
  }
}

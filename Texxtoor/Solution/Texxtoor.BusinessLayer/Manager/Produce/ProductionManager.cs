using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Texxtoor.BaseLibrary.Pdf;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging.Barcode;

namespace Texxtoor.BusinessLayer {


  /// <summary>
  /// This class contains all methods to create actual content, such as PDF, EPub, iBook, and more.
  /// </summary>
  public partial class ProductionManager : Manager<ProductionManager> {

    public ProductionManager() {
    }

    void converter_IssueReport(object sender, KeyValuePair<string, string> e) {
      _issueReport.Add(e.Key, e.Value);
    }

    public void StoreFileReferences(MediaFiles files, int orderProductId) {
      var product = Ctx.OrderProducts.Find(orderProductId);
      if (product != null) {
        product.SetContent(files);
        SaveChanges();
      }
    }

    /// <summary>
    /// This function stores a media file in blob storage and returns a serializable reference object
    /// for the order process.
    /// </summary>
    /// <param name="fileName">The internal name</param>
    /// <param name="group">The media type, such as 'epub', 'pdf'</param>
    /// <param name="content">The actual content</param>
    /// <param name="userName">The user this content is applied to.</param>
    /// <returns></returns>
    public MediaFile StoreMediaFile(string fileName, GroupKind group, byte[] content, string userName) {
      if (content == null) return null;
      var id = Guid.NewGuid();
      using (var blob = BlobFactory.GetBlobStorage(id, BlobFactory.Container.MediaFiles)) {
        blob.Content = content;
        var fileRes = new UserFile {
          Name = fileName,
          Owner = userName == null ? null : GetCurrentUser(userName),
          ResourceId = id,
          Folder = BlobFactory.Container.MediaFiles.ToString(),
          Private = true
        };
        Ctx.UserFiles.Add(fileRes);
        SaveChanges();
        blob.Save();
      }
      return new MediaFile(id, group.ToString().ToLowerInvariant());
    }

    public void SetFullfillmentState(int orderProductId, FullFillmentState state) {
      var product = Ctx.OrderProducts.Find(orderProductId);
      if (product != null) {
        product.Store.FullFillment = state;
        SaveChanges();
      }
    }

    public EventHandler ProductionProgress;

    private void OnProductionProgress() {
      Debug.WriteLine("Production Step");
      if (ProductionProgress != null) {
        ProductionProgress(this, EventArgs.Empty);
      }
    }

    private static byte[] GetBarCode(string code, int w, int h, string title) {
      var bcl = new BarcodeLib(code, BarCodeType.ISBN) {
        Width = w,
        Height = h,
        ForeColor = Color.Black,
        BackColor = Color.White,
        ImageFormat = ImageFormat.Png,
        IncludeLabel = true,
        LabelPosition = LabelPositions.TOPCENTER,
        RawData = title
      };
      bcl.Encode();
      return bcl.GetImageData(SaveTypes.PNG);
    }

    /// <summary>
    ///  Special function for SQLite generation where the copy mechanism doesn't like (and doesn't need) proxies.
    /// </summary>
    /// <param name="docId"></param>
    /// <returns></returns>
    public Element GetDocumentWithoutProxy(int docId) {
      var proxyState = Ctx.Configuration.ProxyCreationEnabled;
      Ctx.Configuration.ProxyCreationEnabled = false;
      var doc = ProjectManager.Instance.GetOpusInternal(docId);
      Ctx.Configuration.ProxyCreationEnabled = proxyState;
      return doc;
    }

    private IDictionary<string, NumberingSchema> GetLocalizedNumberingSchema() {
      return
      new Dictionary<string, NumberingSchema> {        
          {"Section1", new NumberingSchema{
            Major = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Sec1, 
            Divider = ControllerResources.ProductionManager_Div_Sec1, 
            Label = ControllerResources.ProductionManager_Label_Sec1, 
            IncludeParent = false /* on chapter level this options suppresses the chapter number, as we create this in princexml's css */
          }}, 
          {"Section2", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Sec2, 
            Divider = ControllerResources.ProductionManager_Div_Sec2, 
            Label = ControllerResources.ProductionManager_Label_Sec2, 
            IncludeParent = true
          }},
          {"Section3", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Sec3, 
            Divider = ControllerResources.ProductionManager_Div_Sec3, 
            Label = ControllerResources.ProductionManager_Label_Sec3, 
            IncludeParent = true
          }},
          {"Section4", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Sec4, 
            Divider = ControllerResources.ProductionManager_Div_Sec4, 
            Label = ControllerResources.ProductionManager_Label_Sec4, 
            IncludeParent = true
          }},
          {"Section5", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Sec5, 
            Divider = ControllerResources.ProductionManager_Div_Sec5, 
            Label = ControllerResources.ProductionManager_Label_Sec5, 
            IncludeParent = true
          }},
          {"Section6", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Sec6, 
            Divider = ControllerResources.ProductionManager_Div_Sec6, 
            Label = ControllerResources.ProductionManager_Label_Sec6, 
            IncludeParent = true
          }},
          {"ImageSnippet", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Figure, 
            Divider = ControllerResources.ProductionManager_Div_Figure, 
            Label = ControllerResources.ProductionManager_Label_Figure, 
          }}, 
          {"TableSnippet", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Table, 
            Divider = ControllerResources.ProductionManager_Div_Table, 
            Label = ControllerResources.ProductionManager_Label_Table, 
          }},
          {"ListingSnippet", new NumberingSchema{
            Major = 1, 
            Minor = 1, 
            Separator = ControllerResources.ProductionManager_Sep_Listing, 
            Divider = ControllerResources.ProductionManager_Div_Listing, 
            Label = ControllerResources.ProductionManager_Label_Listing, 
          }}
        };
    }

    private static void ClearDataFolder(string path) {
      if (path.EndsWith("\\")) {
        path = path.Substring(0, path.Length - 1);
      }
      if (!Directory.Exists(path)) {
        Directory.CreateDirectory(path);
        return;
      }
      foreach (var file in Directory.GetFiles(path)) {
        try {
          if (File.Exists(file)) {
            File.Delete(file);
          }
        } catch (Exception) {
        }
      }
    }

    # region Create Abstract Production Instruction

    public Printable CreatePrintable(OrderProduct product, string publisher, int templateGroupId) {
      var templates = Ctx.TemplateGroups.Find(templateGroupId);
      var printable = new Printable(templates.Templates.ToList());
      var owner = product.Owner;
      printable.CallingUser = String.Format("{0} ({1})", owner.UserName, owner.Email);
      switch (product.Work.Extern) {
        case WorkType.Custom:
          printable.AuthorImage = null;
          printable.Title = product.Title;
          printable.SubTitle = product.SubTitle;
          printable.PublishingDate = DateTime.Now;
          printable.Publisher = publisher ?? "texxtoor";
          printable.CoverDescription = product.Work.Note;
          printable.AuthorNamesShort = product.Authors;
          // TODO: Add Data
          throw new NotImplementedException();
          return printable;
        case WorkType.External:
          var workBook = product.Work.ExternalBook;
          printable.AuthorImage = null;
          printable.Title = workBook.PackageData.MetaData.Title.Text;
          printable.SubTitle = workBook.PackageData.MetaData.Subject.Text;
          printable.PublishingDate = workBook.PackageData.MetaData.Date.Value;
          printable.Publisher = workBook.PackageData.MetaData.Publisher.Text;
          printable.CoverDescription = workBook.PackageData.MetaData.Description.Text;
          printable.AuthorNamesShort = workBook.PackageData.MetaData.Creator.Text;
          printable.AdditionalAuthorInfo = new List<string>(new[] { workBook.PackageData.MetaData.Contributor.Text });
          // TODO: Add Data
          throw new NotImplementedException();
          return printable;
        case WorkType.Published:
          if (product.Work.Published != null) {
            // we assume that published has templategroup set
            return CreatePrintable(product.Work.Published);
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("product", "unknown Worktype");
      }
      throw new ArgumentException("Could not create anything using the given input");
    }

    private string AuthorForCover (params int[] ids) {
      var names = ids.Select(uid => {
        var user = UserManager.Instance.GetUser(uid).Profile;
        return String.Format("{0}{3}{1}{3}{2}", user.FirstName, user.MiddleName, user.LastName, user.MiddleName != null ? "" : " ");
      }).ToArray();
      return String.Join(" | ", names);
    }

    private string AuthorBiographies (params int[] ids) {
      var names = ids.Select(uid => {
        var user = UserManager.Instance.GetUser(uid).Profile;
        return user.Application;
      }).ToArray();
      return String.Join(" | ", names);
    }

    public Printable CreatePrintable(Published published, TemplateGroup temporaryGroup = null) {
      string applicationPath = CreateTempPath(published.Owner);
      IEnumerable<Template> templates = null;
      if (temporaryGroup == null && published.PreferredTemplateGroup == null) {
        throw new ArgumentOutOfRangeException("templateName");
      }
      if (temporaryGroup == null) {
        templates = published.PreferredTemplateGroup.Single(t => t.Group == GroupKind.Pdf && published.LocaleId == t.LocaleId).Templates;
      } else {
        templates = temporaryGroup.Templates;
      }
      // enrich the printable with marketing settings
      byte[] finalCover;
      using (var ms = new MemoryStream()) {
        published.CoverImage.GetFinalCover(published).Save(ms, ImageFormat.Png);
        ms.Position = 0;
        finalCover = ms.ToArray();
      }      
      var printable = new Printable(templates.ToList()) {
        CallingUser = String.Format("{0} ({1})", published.Owner.UserName, published.Owner.Email),
        AdditionalAuthorInfo = published.AuthorProfiles != null ? published.AuthorProfiles.Where(p => p.Walltext != null).Select(p => p.Walltext).ToList() : new List<string>(),
        AuthorImage = published.Owner.Profile.Image,
        Title = published.Title,
        SubTitle = published.SubTitle,
        Keywords = published.ExternalPublisher.Keywords,
        PublishingDate = published.CreatedAt,
        Publisher = published.Publisher,
        AuthorNamesShort = AuthorForCover(published.ExternalPublisher.AuthorIds.ToArray()),
        AuthorBiography = AuthorBiographies(published.ExternalPublisher.AuthorIds.ToArray()),
        CoverImage = finalCover,
        CoverDescription = published.ExternalPublisher.Description ?? String.Empty,
        ToC = CreateToc(published.SourceOpus),
        TempStorePath = applicationPath
      };
      printable.Images = new List<Printable.ImageFile>();
      printable.Chapters = new List<Printable.ContentFile>();
      // Content
      Ctx.LoadProperty(published, p => p.FrozenFragments);
      Action<IList<FrozenFragment>> publishFrozenFragments = null;
      publishFrozenFragments = fragments => fragments.OrderBy(f => f.OrderNr).ToList().ForEach(f => {
        switch (f.TypeOfFragment) {
          case FragmentType.Html:
            printable.Chapters.Add(new Printable.ContentFile {
              Content = f.Content,
              Name = f.Name,
              Identifier = f.Id.ToString(),
              Href = f.ItemHref
            });
            break;
          case FragmentType.Image:
            printable.Images.Add(new Printable.ImageFile {
              Content = f.Content,
              Name = f.Name,
              Identifier = f.Id.ToString(),
              Href = f.ItemHref
            });
            break;
        }
        // process embedded resources, such as images, which appear as child fragments
        if (f.HasChildren()) {
          publishFrozenFragments(f.Children);
        }
      });
      publishFrozenFragments(published.FrozenFragments);
      // numbering is done by the final style sheet via counter rules
      // as frozen fragments have pure content only we ask opus for builder instructions
      // ?? published.SourceOpus.BuiltContent = sb.ToString();
      return printable;
    }

    public Printable CreatePrintable(Opus opus, User owner, int templateGroupId) {
      // Cover and Common
      var ownerProfile = owner.Profile;
      if (ownerProfile == null)
        throw new ArgumentException("ownerProfile missing");
      // template is used to determin the behavior, then
      var template = Ctx.TemplateGroups.Find(templateGroupId);
      var printable = new Printable(template.Templates.ToList()) {
        TempStorePath = CreateTempPath(owner),
        Title = opus.Name,
        SubTitle = String.IsNullOrEmpty(opus.Project.Description) ? ControllerResources.ProductionManager_CreatePdf_Author_s_Preview : opus.Project.Description.Ellipsis(80).ToString(),
        Keywords = "",
        AuthorNamesShort = AuthorForCover(owner.Id),
        AuthorBiography = AuthorBiographies(owner.Id),
        PublishingDate = opus.CreatedAt,
        Publisher = "Augmented Content GmbH",
        CoverDescription = opus.Project.Description,
        ToC = CreateToc(opus),
        AuthorImage = ownerProfile.Image ?? opus.Project.Image,
        CallingUser = String.Format("{0} ({1})", owner.UserName, owner.Email)
      };      
      // Content
      ClearDataFolder(printable.TempStorePath);
      printable.Images = new List<Printable.ImageFile>();
      var numbering = GetLocalizedNumberingSchema();
      switch (template.Group) {
        case GroupKind.Pdf:
          # region PDF
          // template decides which max width we can use for images
          opus.BuiltContent = CreateDocumentHtml(
                              opus,
                              (sender, args) => {
                                var file = new Printable.ImageFile {
                                  Content = args.Content,
                                  Href = args.FileName
                                };
                                printable.Images.Add(file);
                                // this is the local path the makepdf function uses to retrieve the images (other target groups use different methods) 
                                return Path.Combine(printable.TempStorePath, args.FileName);
                              },
                              (sender, args) => {
                                var p = args.Properties;
                                // TODO: Handle dynamic image properties
                                args.Properties = p;
                              },
                              () => CreateHtmlInner(opus.Children.OfType<Snippet>().OrderBy(c => c.OrderNr), numbering, GroupKind.Pdf),
                              numbering, printable.TempStorePath, GroupKind.Pdf
          );
          // if target is PDF we create one file, in EPUB we make one per chapter
          printable.Chapters = new List<Printable.ContentFile>{
            new Printable.ContentFile(){
              Content = Encoding.UTF8.GetBytes(opus.BuiltContent),
              Href = opus.Name,
              Identifier = opus.Id.ToString(CultureInfo.InvariantCulture)
            }
          };
          # endregion
          break;
        case GroupKind.Epub:
          # region EPUB
          Func<Section, string> createChapter = null;
          createChapter = (s) => CreateChapterHtml(
                              opus,
                              s,
                              (sender, args) => {
                                var file = new Printable.ImageFile {
                                  Content = args.Content,
                                  Href = args.FileName,
                                  Name = args.SourceSnippet.Name,
                                  Identifier = Path.GetFileNameWithoutExtension(args.FileName)
                                };
                                // add just one times even if multiple times used
                                if (printable.Images.All(f => f.Href != args.FileName)) {
                                  printable.Images.Add(file);
                                }
                                // do not modify, we adjust the path while creating the EPUB package
                                return args.FileName;
                              },
                              (sender, args) => {
                                var p = args.Properties;
                                // TODO: Handle dynamic image properties
                                args.Properties = p;
                              },
                              printable.TempStorePath, GroupKind.Epub);
          // if target is PDF we create one file, in EPUB we make one per chapter
          printable.Chapters = new List<Printable.ContentFile>();
          foreach (var section in opus.Children.OfType<Section>()) {
            printable.Chapters.Add(new Printable.ContentFile() {
              Content = Encoding.UTF8.GetBytes(createChapter(section)),
              Href = section.Name,
              Identifier = section.Id.ToString(CultureInfo.InvariantCulture)
            });
          }
          printable.HasAbout = false; // not for epub
          # endregion
          break;
        case GroupKind.Html:
          # region HTML
          opus.BuiltContent = CreateDocumentHtml(
                              opus,
                              (sender, args) => {
                                var file = new Printable.ImageFile {
                                  Content = args.Content,
                                  Href = args.FileName
                                };
                                printable.Images.Add(file);
                                // this creates an inline image 
                                return String.Format("data:image/png;base64,{0}", Convert.ToBase64String(args.Content));
                              },
                              (sender, args) => {
                                var p = args.Properties;
                                // TODO: 
                                args.Properties = p;
                              },
                              () => CreateHtmlInner(opus.Children.OfType<Snippet>().OrderBy(c => c.OrderNr), numbering, GroupKind.Html),
                              numbering, printable.TempStorePath, GroupKind.Html
          );
          printable.Chapters = new List<Printable.ContentFile>{
            new Printable.ContentFile(){
              Content = Encoding.UTF8.GetBytes(opus.BuiltContent),
              Href = opus.Name,
              Identifier = opus.Id.ToString(CultureInfo.InvariantCulture)
            }
          };
          printable.HasAbout = false; // not for opus
          # endregion
          break;
      }
      return printable;
    }

    private List<Printable.TocElement> CreateToc(Element opus) {
      var toc = new List<Printable.TocElement>();
      // Table of Content
      foreach (var chapter in opus.Children.OfType<Section>().OrderBy(c => c.OrderNr)) {
        var t1 = new Printable.TocElement {
          BuilderId = chapter.BuilderId,
          OrderNr = chapter.OrderNr,
          Text = chapter.RawContent
        };
        toc.Add(t1);
        if (!chapter.HasChildren() || !chapter.Children.Any()) continue;
        foreach (var section1 in chapter.Children.OfType<Section>().OrderBy(c => c.OrderNr)) {
          var t2 = new Printable.TocElement {
            BuilderId = section1.BuilderId,
            OrderNr = section1.OrderNr,
            Text = section1.RawContent
          };
          t1.Children.Add(t2);
          if (!section1.HasChildren() || !section1.Children.Any()) continue;
          foreach (var section2 in section1.Children.OfType<Section>().OrderBy(c => c.OrderNr)) {
            var t3 = new Printable.TocElement {
              BuilderId = section2.BuilderId,
              OrderNr = section2.OrderNr,
              Text = section2.RawContent
            };
            t2.Children.Add(t3);
          }
        }
      }
      return toc;
    }


    //private void GetContent(string applicationPath) {
    //  const int imageWidth = 550;
    //  // template decides which max width we can use for images
    //  CreateImageHandler createImagePreview = (sender, arguments) => {
    //    var file = ImageFile.CreateImageFile(Path.GetExtension(arguments.FileName).Substring(1), arguments.Content);
    //    file.Href = "images/" + Path.GetFileNameWithoutExtension(arguments.FileName);
    //    file.Identifier = Path.GetFileNameWithoutExtension(arguments.FileName);
    //    printable.Images.Add(file);
    //    return file.Href;
    //  };
    //  var data = opus.Children.OfType<Section>().Select(chapter => new ContentFile {
    //    Title = chapter.RawContent,
    //    Document = CreateChapterHtml(opus,
    //      chapter,
    //      createImagePreview,
    //      null,
    //      applicationPath,
    //      TemplateGroup.Epub,
    //      false)
    //      .Replace("{Css}", @"<link href=""css/texxtoor.css"" type=""text/css"" rel=""stylesheet"" />")
    //      .Replace("{Title}", @"<title>" + chapter.Name + "</title>"),
    //    Identifier = "txxt" + chapter.Id,
    //    Href = String.Format("chapter_{1}_{0:000}.html", orderNr++, chapter.Name.Replace(" ", "_"))
    //  });
    //  data.ToList().ForEach(d => printable.Chapters.Add(d));

    //}

    /// <summary>
    /// If an authors imageis missing or in poduction step where it is not yet available, we provide pseudo image.
    /// </summary>
    private static byte[] PseudoFace {
      get { return null; }
    }

    private string CreateTempPath(User user) {
      var path = HttpContext.Current.Server.MapPath("~/App_Data/Blobs/Created/" + user.UserName);
      if (!Directory.Exists(path)) {
        Directory.CreateDirectory(path);
      }
      return path + "\\";
    }

    # endregion


    # region Printer Support

    public void SendPdfToPrinter() {

    }

    # endregion

  }
}
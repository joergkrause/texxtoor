using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.Editor.Models;
using Texxtoor.Editor.Utilities;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;
using Texxtoor.Models.Attributes;

namespace Texxtoor.Editor.Core
{
    /// <summary>
    /// This class contains all methods to create actual content, such as PDF, EPub, iBook, and more.
    /// </summary>
    public class ProductionManager : Manager<ProductionManager>
    {
        #region Global Variables
        int ChapterCounter = 0;
        int CssCounter = 1;
        int ImgCounter = 1;
        int FontCounter = 1;
        int PlayCounter = 1;
        int CurrentCounter = 0;
        int ElementIndex = 0;
        int NavIndex = 0;
        string HtmlContent = string.Empty;
        string CsssContent = string.Empty;
        string HtmlTitleContent = string.Empty;
        Manifest manifest = new Manifest();

        List<NavPoint> navList = new List<NavPoint>();
        List<ItemRef> ItemRefList = new List<ItemRef>();

        Document doc = new Document();
        #endregion

        #region Epub Custom Generations

        public EpubBook GenerateEpubMedia(int id)
        {
            doc = ProjectManager.Instance.GetDocument(id);

            if (doc == null) throw new ArgumentNullException("opus");

            manifest.Items = new List<ManifestItem>();

            var book = new EpubBook(); //create epub book object 

            ClearObjectValues(); //reset older values.


            //set values for container.xml to book object
            book.ContainerData = new Container();

            //create & set value for content.opf 
            OpfPackage package = new OpfPackage();

            //set values for opf
            package.Dir = ProgressionDirection.Ltr;
            package.Version = "2.0";
            package.Language = "en";
            package.Identifier = "urn:isbn:9780735656680";
            package.MetaData = GenerateMetaData();
            package.Manifest = GenerateManifest(doc);
            package.Spine = GenerateSpine();
            package.Guide = GenerateGuide();
            book.PackageData = package;

            //create & set values for toc.ncx
            Navigation navigationdata = new Navigation();
            //set Head
            navigationdata.HeadMetaData = GenerateHeadMetaData();
            //set NavMap
            navigationdata.NavMap = navList;
            book.NavigationData = navigationdata;

            book.CoverDescription = book.PackageData.MetaData.Title.Text;
            book.CoverImage = ReadBytesFromFile(HttpContext.Current.Server.MapPath("~/data/Epub") + "\\httpatomoreillycomsourcemspimages741241.jpg");

            return book;
        }

        private void ClearObjectValues()
        {
            manifest.Items.Clear();
            navList.Clear();
            ItemRefList.Clear();
            ChapterCounter = 0;
            CssCounter = 1;
            ImgCounter = 1;
            FontCounter = 1;
            PlayCounter = 1;
            CurrentCounter = 0;
            ElementIndex = 0;
            NavIndex = 0;
            HtmlContent = string.Empty;
            CsssContent = string.Empty;
            HtmlTitleContent = string.Empty;
        }

        private MetaData GenerateMetaData()
        {
            CreatorElement creator = new CreatorElement();
            creator.Text = "Charles Petzold";

            IdentifierElement identifier = new IdentifierElement();
            identifier.Identifier = "urn:isbn:9780735656680";

            TitleElement title = new TitleElement();
            //title.Text = "Microsoft XNA Framework Edition: Programming Windows Phone 7";
            title.Text = doc.Name;

            RightsElement rights = new RightsElement();
            rights.Text = "Copyright © 2010";

            PublisherElement publisher = new PublisherElement();
            publisher.Text = "Microsoft Press";

            SubjectElement subject = new SubjectElement();
            subject.Text = "COMPUTERS / Programming / Microsoft Programming";

            DateElement date = new DateElement();
            date.Text = "2010-12-15";

            DescriptionElement description = new DescriptionElement();
            description.Text = "&lt;p&gt;Focusing on XNA and the C# language, you&amp;#8217;ll learn how to extend your existing skills to the Windows Phone 7 platform&amp;#8212;mastering the core tools and techniques for creating your own games for the phone.&lt;/p&gt;";

            LanguageElement language = new LanguageElement();
            language.Text = "en";

            MetaData meta = new MetaData()
            {
                Identifier = identifier,
                Title = title,
                Rights = rights,
                Publisher = publisher,
                Subject = subject,
                Date = date,
                Description = description,
                Creator = creator,
                Language = language
            };

            return meta;
        }

        private Manifest GenerateManifest(Document doc)
        {
            if (doc.HasChildren())
            {

                ManifestItem CssElement = ManifestItem.Create(FileItemType.CSS);
                CssElement.Href = "css/document.css";
                CssElement.Identifier = "document.css";
                CssElement.Data = ReadBytesFromFile(HttpContext.Current.Server.MapPath("~/data/Epub") + "\\document.css");
                CssElement.MediaType = "text/css";
                manifest.Items.Add(CssElement);
                CsssContent += "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + CssElement.Href + "\"/>";

                ManifestItem ImageElement = ManifestItem.Create(FileItemType.JPEG);
                ImageElement.Href = "httpatomoreillycomsourcemspimages741241.jpeg";
                ImageElement.Identifier = "httpatomoreillycomsourcemspimages741241";
                ImageElement.Data = ReadBytesFromFile(HttpContext.Current.Server.MapPath("~/data/Epub") + "\\httpatomoreillycomsourcemspimages741241.jpg");
                ImageElement.MediaType = "image/jpeg";
                manifest.Items.Add(ImageElement);

                ManifestItem HtmlElement = ManifestItem.Create(FileItemType.XHTML);
                HtmlElement.Href = "cover.html";
                HtmlElement.Identifier = "cover";
                HtmlElement.MediaType = "application/xhtml+xml";
                HtmlElement.Data = ReadBytesFromFile(HttpContext.Current.Server.MapPath("~/data/Epub") + "\\cover.html");
                manifest.Items.Add(HtmlElement);
                ItemRefList.Add(new ItemRef { IdRef = HtmlElement.Identifier, Linear = false });

                GenerateManifestElementsRecursively(doc.Children);
            }
            manifest.Items[ElementIndex].Data = CreatexHtmlPage();
            HtmlContent = string.Empty;
            CurrentCounter = 0;
            ChapterCounter = 0;
            ElementIndex = 0;
            return manifest;
        }

        private void GenerateManifestElementsRecursively(List<Element> Children)
        {
            foreach (Element Child in Children)
            {
                if (Child.Content != null)
                {
                    switch (Child.ProposedFragmentType)
                    {
                        case FragmentType.Audio:
                            break;
                        case FragmentType.Css:
                            ManifestItem CssElement = ManifestItem.Create(FileItemType.CSS);
                            CssElement.Href = "css/css_style_" + CssCounter.ToString("000") + ".css";
                            CssElement.Identifier = "css_style_" + CssCounter.ToString("000");
                            CssElement.Data = Child.Content;
                            CssElement.MediaType = "text/css";
                            manifest.Items.Add(CssElement);
                            CsssContent += "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + CssElement.Href + "\"/>";
                            CssCounter++;
                            break;
                        case FragmentType.Data:
                            break;
                        case FragmentType.Font:
                            ManifestItem FontElement = ManifestItem.Create(FileItemType.OTF);
                            FontElement.Href = "font/epub.embedded.font_" + FontCounter.ToString("000") + ".otf";
                            FontElement.Identifier = "epub.embedded.font_" + FontCounter.ToString("000");
                            FontElement.Data = Child.Content;
                            FontElement.MediaType = "font/opentype";
                            manifest.Items.Add(FontElement);
                            FontCounter++;
                            break;
                        case FragmentType.Html:
                            if (Child.Level == 2)
                            {
                                if (CurrentCounter != ChapterCounter)
                                {
                                    manifest.Items[ElementIndex].Data = CreatexHtmlPage();
                                    HtmlContent = string.Empty;
                                    CurrentCounter = ChapterCounter;
                                }
                                ChapterCounter++;
                                ManifestItem HtmlElement = ManifestItem.Create(FileItemType.XHTML);
                                HtmlElement.Href = "CH" + ChapterCounter.ToString("000") + ".xhtml";
                                HtmlElement.Identifier = "id" + ChapterCounter.ToString("000");
                                HtmlContent += Encoding.UTF8.GetString(Child.Content);
                                HtmlTitleContent = Child.Name;
                                HtmlElement.MediaType = "application/xhtml+xml";
                                manifest.Items.Add(HtmlElement);
                                ElementIndex = manifest.Items.IndexOf(HtmlElement);

                                ItemRefList.Add(new ItemRef { IdRef = HtmlElement.Identifier, Linear = true });

                                NavPoint Nav = new NavPoint();
                                Nav.Identifier = "id" + PlayCounter.ToString("00000");
                                Nav.PlayOrder = PlayCounter;
                                Nav.LabelText = Child.Name;
                                Nav.Content = HtmlElement.Href;
                                Nav.Children = new List<NavPoint>();
                                navList.Add(Nav);
                                NavIndex = navList.IndexOf(Nav);
                                PlayCounter++;
                            }
                            else
                            {
                                if (Child.Level == 3)
                                {
                                    if (Child.WidgetName.Contains("Section"))
                                    {
                                        HtmlContent += "<a id=\"" + Child.Name.Replace(" ", "-") + "\">" + Encoding.UTF8.GetString(Child.Content) + "</a>";
                                        navList[NavIndex].Children.Add(new NavPoint()
                                        {
                                            Identifier = "id" + PlayCounter.ToString("00000"),
                                            PlayOrder = PlayCounter,
                                            LabelText = Child.Name,
                                            Content = manifest.Items[ElementIndex].Href + "#" + Child.Name.Replace(" ", "-")
                                        });
                                        PlayCounter++;
                                    }
                                    else
                                        HtmlContent += Encoding.UTF8.GetString(Child.Content);
                                }
                                else
                                    HtmlContent += Encoding.UTF8.GetString(Child.Content);
                            }
                            break;
                        case FragmentType.Image:
                            if (Child.RawContent.Contains("PNG"))
                            {
                                ManifestItem ImageElement = ManifestItem.Create(FileItemType.PNG);
                                ImageElement.Href = "images/img_" + ImgCounter.ToString("000") + ".png";
                                ImageElement.Identifier = "img_" + ImgCounter.ToString("000");
                                ImageElement.Data = Child.Content;
                                ImageElement.MediaType = "image/png";
                                manifest.Items.Add(ImageElement);
                                HtmlContent += "<p><img src=\"" + ImageElement.Href + "\" alt=\"" + Child.Name + "\"/></p>";
                            }
                            else if (Child.RawContent.Contains("JPG"))
                            {
                                ManifestItem ImageElement = ManifestItem.Create(FileItemType.JPEG);
                                ImageElement.Href = "images/img_" + ImgCounter.ToString("000") + ".jpg";
                                ImageElement.Identifier = "img_" + ImgCounter.ToString("000");
                                ImageElement.Data = Child.Content;
                                ImageElement.MediaType = "image/jpeg";
                                manifest.Items.Add(ImageElement);
                                HtmlContent += "<p><img src=\"" + ImageElement.Href + "\" alt=\"" + Child.Name + "\"/></p>";
                            }
                            else if (Child.RawContent.Contains("GIF"))
                            {
                                ManifestItem ImageElement = ManifestItem.Create(FileItemType.GIF);
                                ImageElement.Href = "images/img_" + ImgCounter.ToString("000") + ".gif";
                                ImageElement.Identifier = "img_" + ImgCounter.ToString("000");
                                ImageElement.Data = Child.Content;
                                ImageElement.MediaType = "image/gif";
                                manifest.Items.Add(ImageElement);
                                HtmlContent += "<p><img src=\"" + ImageElement.Href + "\" alt=\"" + Child.Name + "\"/></p>";
                            }
                            ImgCounter++;
                            break;
                        case FragmentType.Meta:
                            break;
                        case FragmentType.Script:
                            break;
                        case FragmentType.Video:
                            break;
                    }
                }
                if (Child.HasChildren())
                {
                    GenerateManifestElementsRecursively(Child.Children);
                }
            }
        }

        private byte[] CreatexHtmlPage()
        {
            string HtmlBody = string.Empty;
            HtmlBody += "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>";
            HtmlBody += "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">";
            HtmlBody += "<html xmlns=\"http://www.w3.org/1999/xhtml\">";
            HtmlBody += "<head><title>" + HtmlTitleContent + "</title>" + CsssContent + "</head>";
            HtmlBody += "<body><div>";
            HtmlContent = HtmlBody + HtmlContent + "</div></body></html>";
            return Encoding.UTF8.GetBytes(HtmlContent);
        }

        private byte[] ReadBytesFromFile(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }
        }

        private Spine GenerateSpine()
        {
            Spine spine = new Spine()
            {
                Toc = "ncx",
                ItemRefs = ItemRefList
            };
            return spine;
        }

        private Guide GenerateGuide()
        {
            Guide guide = new Guide();
            References reference = new References();
            guide.ReferenceHref = "cover.html";
            guide.ReferenceType = "cover";
            guide.ReferenceTitle = "Cover";
            return guide;
        }

        private Head GenerateHeadMetaData()
        {
            Head headmetadata = new Head()
            {
                Identifier = "isbn:9780735656680",
                Depth = 0,
                TotalPageCount = 0,
                MaxPageNumber = 0,
            };
            return headmetadata;
        }

        #endregion
    }
}
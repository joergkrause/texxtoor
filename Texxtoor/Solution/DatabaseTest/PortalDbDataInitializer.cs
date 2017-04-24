using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Repository;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Model.Cms.Localization;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.JobPortal;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.BaseLibrary.Services;

namespace Texxtoor.DataBase.Initializer {

  public class PortalDbDataInitializer : DropCreateDatabaseAlways<PortalContext> {

    # region AspNet User Management

    protected UserManager<User, int> Usermanager;
    protected RoleManager<Role, int> Rolemanager;

    public PortalDbDataInitializer() {
    }

    public PortalDbDataInitializer(PortalContext ctx) {
      context = ctx;
      Usermanager = new UserManager<User, int>(new Manager<UserManager>.TexxtoorUserStore<User>(ctx));
      Usermanager.PasswordHasher = new TexxtoorPasswordHasher(TexxtoorMembershipService.HashAlgorithmType);
      Rolemanager = new RoleManager<Role, int>(new Manager<UserManager>.TexxtoorRoleStore<Role>(ctx));
    }


    # endregion


    internal string TargetDeployment;

    protected override void Seed(PortalContext ctx) {
      base.Seed(ctx);
      context = ctx;
      Initialize(TargetDeployment);
    }

    private static PortalContext context;

    public void Initialize(string targetDeployment) {
      if (targetDeployment == null) throw new ArgumentNullException();
      TargetDeployment = targetDeployment;
      #region Reader Portal
      Console.WriteLine("Creating Root App ...");
      var app = CreateRootApp();
      context.SaveChanges();
      Console.Write("Res,");
      LoadLocaleResources();
      Console.Write("Countries,");
      LoadCountries();
      Console.Write("Roles,");
      LoadRoles();
      Console.Write("Users,");
      LoadUsers();
      // Get Admin
      var adminUser = context.Users.FirstOrDefault(u => u.UserName.Equals("Admin"));
      // Save content as Admin
      Console.WriteLine("CmsData");
      LoadCmsData(app, adminUser, "T");
      LoadCatalog(app);
      #endregion


      #region Author Portal

      Console.WriteLine("Creating Author ...");
      //////////////////////////////////
      // Teams

      var aspNetTeam = new Team {
        Active = true,
        Name = "Windows Authors",
        Description = "TeamLead und Author write about Windows",
        Image = GetImage("team_128.png", "images", ImageFormat.Png)
      };
      context.Teams.Add(aspNetTeam);

      //////////////////////////////////
      // Team Roles

      var tr1 = new TeamRole {
        ContributorRoles = ContributorRole.Author,
        Team = aspNetTeam
      };
      var tr2 = new TeamRole {
        ContributorRoles = ContributorRole.Author,
        Team = aspNetTeam
      };

      (new[] { tr1, tr2 }).ForEach(e => context.TeamRoles.Add(e));

      //////////////////////////////////
      // People in team
      var teamlead = context.Users.Single(u => u.UserName == "Teamlead");
      var author = context.Users.Single(u => u.UserName == "Author");

      var member1 = new TeamMember {
        Role = tr1,
        Member = teamlead,
        Team = aspNetTeam,
        TeamLead = true
      };

      var member2 = new TeamMember {
        Role = tr2,
        Member = author,
        Team = aspNetTeam,
        TeamLead = false
      };

      (new[] { member1, member2 }).ForEach(m => context.TeamMembers.Add(m));
      Console.Write("DemoContent,");
      //////////////////////////////////      
      #region create a sample opus with images
      Project prj = null;
      Func<IEnumerable<XElement>, List<Element>> helper = null;
      string currentChapter = String.Empty;
      int chapterOrder = 1;
      helper = nodes => {
        var ret = new List<Element>();
        int orderNr = 1;
        foreach (var elm in nodes) {
          Element newElm = null;
          # region Detect Element Type
          switch (elm.Attribute("Type").Value.ToLower()) {
            case "opus":
              # region OPUS
              // create project for opus
              prj = new Project {
                Active = true,
                Team = aspNetTeam,
                Name = elm.Attribute("Name").Value + " Project",
                Short = elm.Attribute("Short").Value,
                Description = elm.Attribute("Description").Value
              };
              var tm = member1;
              // create opus
              newElm = new Opus {
                Version = 0,
                Project = prj,
                Name = elm.Attribute("Name").Value
              };
              var opus = (Opus)newElm;
              // this is an easy way to implement a workflow
              // TODO: Make this configurable
              var ms = new List<Milestone>();
              var m1 = new Milestone { Id = 1, Name = "Writing", Description = "Create Content", DateDue = DateTime.Now.AddDays(30), DateAssigned = DateTime.Now, Owner = tm, Progress = 0, Opus = opus };
              var m2 = new Milestone { Id = 2, Name = "Illustrations", Description = "Create Illustrations", DateDue = DateTime.Now.AddDays(40), DateAssigned = DateTime.Now, Owner = tm, Progress = 0, Opus = opus };
              var m3 = new Milestone { Id = 3, Name = "CopyEditing", Description = "Copy Editing", DateDue = DateTime.Now.AddDays(50), DateAssigned = DateTime.Now, Owner = tm, Progress = 0, Opus = opus };
              m2.NextMilestone = m3;
              var m4 = new Milestone { Id = 4, Name = "Proof", Description = "Proof Read", DateDue = DateTime.Now.AddDays(55), DateAssigned = DateTime.Now, Owner = tm, Progress = 0, Opus = opus };
              m3.NextMilestone = m4;
              var m5 = new Milestone { Id = 5, Name = "Marketing", Description = "Create Marketing Package", DateDue = DateTime.Now.AddDays(56), DateAssigned = DateTime.Now, Owner = tm, Progress = 0, Opus = opus };
              ms.AddRange(new Milestone[] { m1, m2, m3, m4, m5 });
              opus.Milestones = ms;
              // Ratios
              opus.ContributorRatios = new List<ContributorRatio>(new ContributorRatio[]  { 
                new ContributorRatio { Contributor = teamlead, ShareType = ShareType.Ratio, ValueOrRatio = 0.50M },
                new ContributorRatio { Contributor = author, ShareType = ShareType.Ratio, ValueOrRatio = 0.50M }
              });
              break;
              # endregion
            case "section":
              # region SECTION
              if (elm.FirstNode != null && elm.FirstNode.NodeType == System.Xml.XmlNodeType.Text) {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes(((XText)elm.FirstNode).Value.Trim())
                };
              } else {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes("Empty Section")
                };
              }
              if (elm.Attribute("Name") == null || String.IsNullOrEmpty(elm.Attribute("Name").Value)) {
                newElm.Name = System.Text.Encoding.UTF8.GetString(newElm.Content);
              } else {
                newElm.Name = elm.Attribute("Name").Value;
                if (elm.FirstNode == null) {
                  newElm.Content = System.Text.Encoding.UTF8.GetBytes(elm.Attribute("Name").Value);
                }
              }
              // Detect Chapter Elements to store resources in subfolders
              if (elm.Parent.Attribute("Type").Value == "Opus") {
                currentChapter = elm.Attribute("Name").Value;
              }
              break;
              # endregion
            case "text":
              # region TEXT
              newElm = new TextSnippet {
                Content = System.Text.UTF8Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Value.CleanUpString(15)
              };
              break;
              # endregion
            case "image":
              # region IMAGE
              var imgpath = elm.Value.Trim();
              Debug.Assert(Path.GetExtension(imgpath).Length > 0, "no extension " + imgpath);
              // get and optionally create folder after chapter
              var currentChapterResFolder = context.Resources
                .OfType<ResourceFolder>()
                .FirstOrDefault(rf => rf.Name == currentChapter);
              if (currentChapterResFolder == null) {
                currentChapterResFolder = new ResourceFolder {
                  Name = currentChapter,
                  Owner = member1.Member,
                  Project = prj,
                  TypesOfResource = TypeOfResource.Content,
                  OrderNr = chapterOrder++
                };
                context.Resources.Add(currentChapterResFolder);
                context.SaveChanges();
              }
              //
              var res = new ResourceFile {
                Owner = member1.Member,
                Name = elm.Attribute("Name").Value,
                Project = prj,
                Parent = currentChapterResFolder,
                ResourceId = Guid.NewGuid(),
                TypesOfResource = TypeOfResource.Content,
                MimeType = "image/" + Path.GetExtension(imgpath).Substring(1) // kick the leading "."
              };
              context.Resources.Add(res);
              var blobImg = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources);
              System.Drawing.Image img = null;
              if (imgpath.StartsWith("http")) {
                blobImg.Content = ReadWebResource(imgpath);
                try {        // unsupported image format. when image is svg
                  using (var imgms = new MemoryStream(blobImg.Content)) {
                    img = System.Drawing.Image.FromStream(imgms);
                  }
                } catch {
                  img = null;
                }
              } else {
                var localPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "content", imgpath);
                if (File.Exists(localPath)) {
                  var bytes = File.ReadAllBytes(localPath);
                  blobImg.Content = bytes;
                  try {        // unsupported image format. when image is svg
                    using (var imgms = new MemoryStream(bytes)) {
                      img = System.Drawing.Image.FromStream(imgms);
                    }
                  } catch {
                    img = null;
                  }
                }
              }
              if (blobImg.Content != null) {
                blobImg.Save();
              }
              newElm = new ImageSnippet {
                // images and other resources are always stored in the BLOB store and as a hard copy in the elements table
                Content = blobImg.Content,
                Name = res.Name,
                Title = res.Name,
                MimeType = res.MimeType
              };
              var imgprops = new ImageProperties();
              if (elm.Attribute("Width") == null || elm.Attribute("Height") == null) {
                if (img != null) {
                  imgprops.ImageWidth = imgprops.OriginalWidth = img.Width;
                  imgprops.ImageHeight = imgprops.OriginalHeight = img.Height;
                } else {
                  imgprops.ImageWidth = imgprops.OriginalWidth = 100;
                  imgprops.ImageHeight = imgprops.OriginalHeight = 100;

                }
              } else {
                imgprops.ImageWidth = imgprops.OriginalWidth = Convert.ToInt32(elm.Attribute("Width").NullSafeString());
                imgprops.ImageHeight = imgprops.OriginalHeight = Convert.ToInt32(elm.Attribute("Height").NullSafeString());
              }
              ((ImageSnippet)newElm).Properties = new JavaScriptSerializer().Serialize(imgprops);
              break;
              # endregion
            case "listing":
              # region LISTING
              newElm = new ListingSnippet {
                Content = System.Text.UTF8Encoding.UTF8.GetBytes(elm.Value.Trim()), //.Replace("\n", " "); - Causes problems with Listing widget. All data is displayed in one line
                Name = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Language = elm.Attribute("Language") == null ? "" : elm.Attribute("Language").Value,
                SyntaxHighlight = elm.Attribute("Highlight") == null ? true : Boolean.Parse(elm.Attribute("Highlight").Value),
                LineNumbers = elm.Attribute("LineNumbers") == null ? true : Boolean.Parse(elm.Attribute("LineNumbers").Value)
              };
              # endregion
              break;
            case "table":
              # region TABLE
              newElm = new TableSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                RepeatHeadRow = elm.Attribute("RepeatHeadRow") == null ? true : Boolean.Parse(elm.Attribute("RepeatHeadRow").Value)
              };
              # endregion
              break;
            case "sidebar":
              # region SIDEBAR
              newElm = new SidebarSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Sidebar" : elm.Attribute("Name").Value,
                SidebarType = elm.GetEnumAttribute<SidebarType>("SidebarType")
              };
              # endregion
              break;
            default:
              throw new NotSupportedException("Unknown snippet type found in source XML: " + elm.Attribute("Type").NullSafeString());
          }
          # endregion
          newElm.Children = helper(elm.Elements("Element"));
          newElm.OrderNr = orderNr++;
          ret.Add(newElm);
        }
        return ret;
      };
      // invoke Content loader (assume each xml contains one Opus)
      try {
        foreach (var importFile in Directory.GetFiles("content")) {
          var doc = XDocument.Load(importFile);
          helper(from o in doc.Root.Elements("Element") select o).ForEach(o => context.Elements.Add(o));
        }
        context.SaveChanges();
        // copy blobs to target project for immediate reference

        // JK: 1/15/2015 this is no longer necessary as the blob storage factory is aware of the target

        //var trgtPth = Directory.GetParent(Path.GetDirectoryName(GetType().Assembly.Location)).Parent.Parent.FullName;
        //foreach (var file in System.IO.Directory.GetFiles(Path.Combine(trgtPth, "Texxtoor.Portal", @"App_Data\Blobs\Resources"), "*.dat")) {
        //  File.Delete(file);
        //}
        //foreach (var file in System.IO.Directory.GetFiles(@"App_Data\Blobs\Resources", "*.dat")) {
        //  File.Copy(file, Path.Combine(trgtPth, "Texxtoor.Portal", file));
        //}
      } catch (Exception ex) {
        Console.WriteLine(ex.Message + " > " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
        throw;
      }
      # endregion

      #endregion
      Console.Write("Terms,");
      # region Terms

      var ts1 = new TermSet {
        LocaleId = "en",
        Name = "Master Termset",
        Description = "This is the master termset for books about computer languages",
        Shared = true,
        Project = prj,
        Terms = new List<Term>(new Term[] {
          new AbbreviationTerm {  Text = "Visual Basic", Content= "VB", LocaleId = "en" },
          new AbbreviationTerm {  Text = "Turbo Pascal", Content= "TP", LocaleId = "en" },
          new AbbreviationTerm {  Text = "C Sharp", Content= "c#", LocaleId = "en" },
          new CiteTerm {  Text = "ASPHanser2010", Content= "ASP.NET 4 Carl Hanser Verlag 2010", LocaleId = "en" },
          new CiteTerm {  Text = "ASPApress2009", Content= "ASP.NET Extensibility Apress 2009", LocaleId = "en" },
          new VariableTerm {  Text = "UserName", Content= "Member.UserName", LocaleId = "" },    // neutral culture
          new VariableTerm {  Text = "UserMail", Content= "Member.UserEmail", LocaleId = "" },   // neutral culture
        })
      };

      var ts2 = new TermSet {
        LocaleId = "en",
        Project = null,   // this termset is "private" to its owner, but not yet assigned to a project
        Name = "Additional Termset",
        Description = "This is an additional private termset for books about c#",
        Shared = false,
        Terms = new List<Term>(new Term[] {
          new AbbreviationTerm {  Text = "Visual Basic (glob)", Content= "VB", LocaleId = "en" },
          new AbbreviationTerm {  Text = "Turbo Pascal (glob)", Content= "TP", LocaleId = "en" },
          new AbbreviationTerm {  Text = "C Sharp (glob)", Content= "c#", LocaleId = "en" }
        })
      };

      context.TermSets.Add(ts1);
      context.TermSets.Add(ts2);

      # endregion
      Console.Write("Messages,");
      # region Common Contribution, Messaging

      /* Messages */
      var msg1 = new Message {
        Sender = author,
        Receiver = teamlead,
        Subject = "Hello TeamLead",
        Body = "This is a message from Author ;-)"
      };

      var msg2 = new Message {
        Sender = teamlead,
        Receiver = author,
        Subject = "Hi Author",
        Body = "This is a message from Teamlead ;-)"
      };
      // this is a global broadcast
      var msg3 = new Message {
        Sender = null,
        Receiver = null,
        Subject = "Dear Users",
        Body = "Welcome to texxtoor - the new textbook publishing platform"
      };

      context.Messages.Add(msg1);
      context.Messages.Add(msg2);
      context.Messages.Add(msg3);

      # endregion
      Console.Write("Workflow,");
      # region Workflow

      LoadWorkflowStepsAndCreateWorkflows();

      # endregion
      Console.Write("Common,");
      # region Common
      Console.WriteLine("Creating Common ...");

      CmsMedia mediaPdf = new CmsMedia { Name = "PDF" };
      CmsMedia mediaEpub = new CmsMedia { Name = "EPub" };
      CmsMedia mediaiPad = new CmsMedia { Name = "iPad" };
      context.Media.Add(mediaPdf);
      context.Media.Add(mediaEpub);
      context.Media.Add(mediaiPad);

      # endregion
      Console.Write("Social,");
      # region Social
      Console.WriteLine("Creating Sozial ...");

      LoadThemes();

      LoadGroups();

      Console.WriteLine("Order");
      # endregion
      # region Order
      Console.WriteLine("Creating Order ...");

      LoadMedia();

      # endregion
      # region Tenants

      CreateTenants();
      # endregion
      CreateMailTemplate();
      CreateJobCategories();
      context.SaveChanges();
    }

    public Application CreateRootApp() {
      var app = context.Applications.SingleOrDefault(a => a.ApplicationName == "/");
      if (app == null) {
        app = context.Applications.Add(new Application {
          Description = "Root",
          ApplicationName = "/"
        });
        context.SaveChanges();
      }
      return app;
    }

    public void CreateJobCategories() {
      var xDoc = XDocument.Load("jobs/jobcategories.xml");
      var catList = xDoc.Root.Elements("category")
        .Select(c => new JobCategory {
          Name = c.Attribute("name").Value,
          LocaleId = c.Attribute("locale").Value,
          Description = c.Value
        }).ToList();
      catList.ForEach(c => context.JobCategories.Add(c));
      context.SaveChanges();
    }

    public void CreateMailTemplate() {
      # region Mail Templates
      // template cascade deleted
      context.TemplateGroups.Where(t => t.Group == GroupKind.Email).ToList().ForEach(t => context.TemplateGroups.Remove(t));
      context.SaveChanges();
      var adminUser = context.Users.First(u => u.UserName == "Admin");
      foreach (var file in Directory.GetFiles("..\\..\\Templates\\" + TargetDeployment + "\\Mail")) {
        var mailDoc = XDocument.Load(file);
        var group = new TemplateGroup {
          Admin = adminUser,
          LocaleId = mailDoc.Root.Attribute("locale").Value,
          Name = mailDoc.Root.Attribute("name").Value,
          Owner = null,
          Group = GroupKind.Email
        };
        var templates = mailDoc.Root.Elements("template").Select(e => new Template {
          Content = Encoding.UTF8.GetBytes(e.Value),
          InternalName = e.Attribute("name").Value,
          Group = group
        }).ToList();
        group.Templates = templates;
        context.TemplateGroups.Add(group);
      }
      context.SaveChanges();

      # endregion

    }

    private void CreateTenants() {
      var app = context.Applications.Single(a => a.ApplicationName == "ac2");
      Console.WriteLine("Create Tenants");
      var tAdmin1 = new User {
        UserName = "T1Admin",
        Password = "p@ssw0rd",
        Comment = "Admin for Tenant",
        Email = "joerg@krause.net"
      };
      tAdmin1.Application = app;
      var tAdmin2 = new User {
        UserName = "T2Admin",
        Password = "p@ssw0rd",
        Comment = "Admin for Tenant",
        Email = "joerg@krause.net"
      };
      tAdmin2.Application = app;
      context.Users.Add(tAdmin1);
      context.Users.Add(tAdmin2);
      var t1 = new Tenant { Name = "Soleosoft", Description = "Software für Krankenhäuser" };
      var t2 = new Tenant { Name = "Medialinx", Description = "Videoschulungen" };
      context.Tenants.Add(t1);
      context.Tenants.Add(t2);
      t1.Users = new List<User>();
      t2.Users = new List<User>();
      t1.Users.Add(tAdmin1);
      t2.Users.Add(tAdmin2);
      context.SaveChanges();
      # region Editor
      foreach (var file in Directory.GetFiles("..\\..\\Templates\\" + TargetDeployment + "\\Editor")) {
        // TODO: Editor Templates
      }
      # endregion
      CreateTemplates(tAdmin1, tAdmin2, t1, t2);
    }

    private static byte[] ReadAllByteNoBOM(string file) {
      byte[] noBom, bom;
      bom = File.ReadAllBytes(file);
      var bl = bom.Length;
      noBom = new Byte[bl - 3];
      Array.Copy(bom, 3, noBom, 0, bl - 3);
      return noBom;
    }


    public void CreateTemplates(User tAdmin1, User tAdmin2, Tenant t1, Tenant t2) {
      Console.WriteLine("Create Templates for Tenants");
      context.TemplateGroups.Where(t => t.Group == GroupKind.Pdf).ToList().ForEach(t => context.TemplateGroups.Remove(t));
      context.SaveChanges();
      # region Pdf

      foreach (var language in Directory.GetDirectories("..\\..\\Templates\\" + TargetDeployment + "\\Pdf")) {
        foreach (var fileGroup in Directory.GetDirectories(language)) {
          var templateGroup = new TemplateGroup {
            Admin = tAdmin1,
            LocaleId = Path.GetFileNameWithoutExtension(language),
            Name = Path.GetFileNameWithoutExtension(fileGroup),
            Owner = null,
            Group = GroupKind.Pdf
          };
          context.TemplateGroups.Add(templateGroup);
          foreach (var file in Directory.GetFiles(fileGroup)) {
            var noTenant = new Template {
              Content = ReadAllByteNoBOM(file),
              InternalName = Path.GetFileName(file),
              Group = templateGroup
            };
            if (noTenant.InternalName == "document.xml") {
              using (var ms = new MemoryStream(noTenant.Content)) {
                var xDoc = XDocument.Load(ms);
                templateGroup.Description = xDoc.Root.Descendants("{http://www.w3.org/1999/xhtml}" + "meta")
                  .Single(e => e.Attribute("name") != null && e.Attribute("name").Value == "description")
                  .Attribute("content").Value;
              }
            }
            context.Templates.Add(noTenant);
            templateGroup.Templates.Add(noTenant);
          }
        }
      }
      # endregion
      context.SaveChanges();
      context.TemplateGroups.Where(t => t.Group == GroupKind.Epub).ToList().ForEach(t => context.TemplateGroups.Remove(t));
      context.SaveChanges();
      # region EPub

      foreach (var language in Directory.GetDirectories("..\\..\\Templates\\" + TargetDeployment + "\\Epub")) {
        foreach (var fileGroup in Directory.GetDirectories(language)) {
          var templateGroup = new TemplateGroup {
            Admin = tAdmin2,
            LocaleId = Path.GetFileNameWithoutExtension(language),
            Name = Path.GetFileNameWithoutExtension(fileGroup),
            Owner = null,
            Group = GroupKind.Epub
          };
          foreach (var file in Directory.GetFiles(fileGroup)) {
            var noTenant = new Template {
              Content = File.ReadAllBytes(file),
              InternalName = Path.GetFileName(file),
              Group = templateGroup
            };
            if (noTenant.InternalName == "document.xml") {
              using (var ms = new MemoryStream(noTenant.Content)) {
                var xDoc = XDocument.Load(ms);
                templateGroup.Description = xDoc.Root.Descendants("{http://www.w3.org/1999/xhtml}" + "meta")
                  .Single(e => e.Attribute("name") != null && e.Attribute("name").Value == "description")
                  .Attribute("content").Value;
              }
            }
            context.Templates.Add(noTenant);
            templateGroup.Templates.Add(noTenant);
          }
        }
      }
      # endregion
      context.SaveChanges();
      context.TemplateGroups.Where(t => t.Group == GroupKind.Html).ToList().ForEach(t => context.TemplateGroups.Remove(t));
      context.SaveChanges();
      # region Html

      foreach (var language in Directory.GetDirectories("..\\..\\Templates\\" + TargetDeployment + "\\Html")) {
        foreach (var fileGroup in Directory.GetDirectories(language)) {
          var templateGroup = new TemplateGroup {
            Admin = tAdmin2,
            LocaleId = Path.GetFileNameWithoutExtension(language),
            Name = Path.GetFileNameWithoutExtension(fileGroup),
            Owner = null,
            Group = GroupKind.Html
          };
          foreach (var file in Directory.GetFiles(fileGroup)) {
            var noTenant = new Template {
              Content = File.ReadAllBytes(file),
              InternalName = Path.GetFileName(file),
              Group = templateGroup
            };
            if (noTenant.InternalName == "document.xml") {
              using (var ms = new MemoryStream(noTenant.Content)) {
                var xDoc = XDocument.Load(ms);
                templateGroup.Description = xDoc.Root.Descendants("{http://www.w3.org/1999/xhtml}" + "meta")
                  .Single(e => e.Attribute("name") != null && e.Attribute("name").Value == "description")
                  .Attribute("content").Value;
              }
            }
            context.Templates.Add(noTenant);
            templateGroup.Templates.Add(noTenant);
          }
        }
      }
      # endregion
      context.SaveChanges();
    }

    public void LoadLocaleResources() {
      Console.Write("Deleting old resources...");
      Console.WriteLine(" {0} resources to remove", context.Localization.Count());
      context.Localization.ToList().ForEach(context.DeleteObject);
      context.SaveChanges();
      Console.WriteLine(" {0} resources left", context.Localization.Count());
      Console.WriteLine("done");
      Console.Write("Adding new resources...");
      foreach (var file in Directory.GetFiles("localize/" + TargetDeployment, "Localization*.xml")) {
        var xLoc = XDocument.Load(file);
        var q1 = xLoc.Root.Elements("loc").Select(LocalizationResource).ToList();
        q1.ForEach(l => context.Localization.Add(l));
        context.SaveChanges();
        Console.WriteLine(" {0} resources added from {1}", q1.Count(), Path.GetFileName(file));
      }
      Console.WriteLine(" {0} resources in store now", context.Localization.Count());
      Console.WriteLine("done");
    }

    private static readonly string ResAssembly = typeof(ResourceBase).Assembly.FullName;

    private ResourceBase LocalizationResource(XElement loc) {
      var type = String.Format("Texxtoor.DataModels.Model.Cms.Localization.{1}, {0}", ResAssembly, loc.Attribute("type").NullSafeString());
      var res = Type.GetType(type).GetConstructor(new Type[0]).Invoke(null) as ResourceBase;
      res.LocaleId = loc.Attribute("localeid").NullSafeString();
      res.ResourceId = loc.Attribute("resid").NullSafeString();
      res.ResourceSet = loc.Attribute("set").NullSafeString();
      res.Value = loc.Element("data").NullSafeString();
      return res;
    }

    public void LoadCountries() {
      // need to do this manually in SQL because of a circular constraint between city and country
      var sql = new List<string>(new string[] {
        "ALTER TABLE Common.Country DROP CONSTRAINT [FK_Common.Country_Common.City_Capital_Id]",
        "ALTER TABLE Common.City DROP CONSTRAINT [FK_Common.City_Common.Country_Country_Id]",
        "ALTER TABLE Common.[Language] DROP CONSTRAINT [FK_Common.Language_Common.Country_Country_Id]",
        "TRUNCATE TABLE Common.COuntry",
        "TRUNCATE TABLE Common.City",
        "TRUNCATE TABLE Common.Language",
        "ALTER TABLE Common.Country ADD CONSTRAINT [FK_Common.Country_Common.City_Capital_Id] FOREIGN KEY (Capital_Id) REFERENCES Common.City(Id)",
        "ALTER TABLE Common.City ADD CONSTRAINT [FK_Common.City_Common.Country_Country_Id] FOREIGN KEY (Country_Id) REFERENCES Common.Country(Id)",
        "ALTER TABLE Common.[Language] ADD CONSTRAINT [FK_Common.Language_Common.Country_Country_Id] FOREIGN KEY (Country_Id) REFERENCES Common.[Country](Id)"
      });
      sql.ForEach(s => context.Database.ExecuteSqlCommand(s));
      var xDoc = XDocument.Load("lang/countries.xml");
      var xCty = XDocument.Load("lang/cities.xml");
      var xLng = XDocument.Load("lang/countrylanguages.xml");
      var query = xDoc.Root.Elements("country")
        .Select(c => new Country {
          Name = c.Element("name").Value,
          Continent = c.Element("continent").Value,
          Region = c.Element("region").Value,
          SurfaceArea = Double.Parse(c.Element("surfacearea").Value),
          Population = Int64.Parse(c.Element("population").Value),
          GNP = Int64.Parse(c.Element("gnp").Value),
          LocalName = c.Element("localname").Value,
          Capital = GetCapitalForCountry(xCty, c.Element("capital").Value),
          Cities = GetCitiesForCountry(xCty, c.Attribute("code").Value),
          IsoCode = c.Attribute("code").Value,
          Iso2Code = c.Attribute("code2").Value,
          Languages = GetLanguagesForCountry(xLng, c.Attribute("code").Value)
        });
      query.ToList().ForEach(c => context.Countries.Add(c));

      context.SaveChanges();

    }

    private List<City> GetCitiesForCountry(XDocument cities, string cntry) {
      var ctys = cities.Root
        .Elements("city")
        .Where(e => e.Attribute("code").Value == cntry)
        .Select(e => new City {
          Name = e.Element("name").NullSafeString(),
          Population = Int64.Parse(e.Attribute("population").NullSafeString()),
          District = e.Element("district").NullSafeString()
        });
      return ctys.ToList();
    }

    private City GetCapitalForCountry(XDocument cities, string cntryId) {
      if (String.IsNullOrEmpty(cntryId)) return null;
      var e = cities.Root.Elements().ElementAt(Int32.Parse(cntryId));
      var cty = new City {
        Name = e.Element("name").NullSafeString(),
        Population = Int64.Parse(e.Attribute("population").NullSafeString()),
        District = e.Element("district").NullSafeString()
      };
      return cty;
    }

    private List<Language> GetLanguagesForCountry(XDocument lang, string cntry) {
      var de = new CultureInfo("de-de");
      var ctys = lang.Root
        .Elements("language")
        .Where(e => e.Attribute("code").Value == cntry)
        .Select(e => new Language {
          Name = e.Element("language").Value,
          Percentage = e.Attribute("percentage") == null ? 0F : Single.Parse(e.Attribute("percentage").NullSafeString(), de),
          IsOfficial = Boolean.Parse(e.Element("official").Value)
        });
      return ctys.ToList();
    }

    private byte[] ReadWebResource(string url) {
      var client = new WebClient();
      try {
        Console.WriteLine("Download file {0} from web", url);
        var data = client.DownloadData(url);
        return data;
      } catch (Exception) {
        return null;
      }
    }

    private void LoadWorkflowStepsAndCreateWorkflows() {
      XDocument xDoc = XDocument.Load("Workflows.xml");
      var query = xDoc
          .Root
          .Elements("workflow")
          .Select(w => new {
            Workflow = w,
            Steps = w.Descendants("step")
          });
      query.ForEach(w => CreateWorkflowSteps(w.Workflow, w.Steps));
      context.SaveChanges();
    }

    private void CreateWorkflowSteps(XElement w, IEnumerable<XElement> steps) {
      var name = w.Attribute("name").NullSafeString();
      var lang = w.Attribute("lang").NullSafeString();
      foreach (var step in steps) {
        WorkflowStepDefinition lastStep = null;
        var roleName = step.Attribute("roles").NullSafeString();
        var workflowStep = new WorkflowStepDefinition {
          WorkflowSet = name,
          Description = step.Value,
          LocaleId = lang,
          Name = step.Attribute("name").NullSafeString(),
          Roles = context.Roles.ToList().Where(r => r.UserRole.ToString() == roleName).ToList(),
          RequiredStep = Boolean.Parse(step.Attribute("required").NullSafeString())
        };
        context.WorkflowStepDefinitions.Add(workflowStep);
      }
    }

    public void LoadMedia() {
      XDocument xDoc = XDocument.Load("OrderMedia.xml");
      var query = xDoc
          .Root
          .Descendants
          ("Media")
          .Select(r => new OrderMedia {
            Name = r.Value,
            PriceBase = Decimal.Parse(r.Attribute("PriceBase").NullSafeString())
          });
      query.ForEach(m => context.OrderMedias.Add(m));
      context.SaveChanges();
    }

    private void LoadThemes() {
      XDocument xDoc = XDocument.Load("Themes.xml");
      var query = xDoc
          .Root
          .Elements("theme")
          .Select(r => new Theme {
            Name = r.Attribute("name").NullSafeString(),
            Description = r.Value
          });
      query.ForEach(t => context.Themes.Add(t));
      context.SaveChanges();
    }

    // pre-load some groups and their members (readergroups.xml)
    private void LoadGroups() {
      XDocument xDoc = XDocument.Load("ReaderGroups.xml");
      var query = xDoc
          .Root
          .Elements("readergroup")
          .Select(r => new ReaderGroup {
            Name = r.Element("name").NullSafeString(),
            Description = r.Element("description").NullSafeString(),
            Closed = Boolean.Parse(r.Element("closed").NullSafeString()),
            Public = Boolean.Parse(r.Element("public").NullSafeString()),
            Members = LoadGroupMembers(r.Element("members").Elements("user")),
            Admins = LoadGroupMembers(r.Element("admins").Elements("user")),
            Owner = LoadGroupOwner(r.Element("owner").Value),
            Themes = LoadThemes(r.Element("themes").Elements("theme"))
          });
      query.ForEach(group => context.ReaderGroups.Add(group));
      context.SaveChanges();
    }

    private static List<User> LoadGroupMembers(IEnumerable<XElement> users) {
      return users.Select(user => context.Users.First(u => u.UserName == user.Value)).ToList();
    }

    private static User LoadGroupOwner(string userName) {
      return context.Users.First(u => u.UserName == userName);
    }

    private static List<Theme> LoadThemes(IEnumerable<XElement> themes) {
      var themeList = new List<Theme>();
      foreach (var theme in themes) {
        var themeObject = context.Themes.FirstOrDefault(t => t.Name == theme.Value);
        themeList.Add(themeObject);
      }
      return themeList;
    }

    public void LoadCmsData(Application app, User author, string targetDeployment) {
      # region assure emptying first for proper refresh
      context.Menus.Include(m => m.Roles).Include(m => m.MenuItems).ForEach(m => m.Roles.Clear());
      context.Menus.ForEach(m => context.Menus.Remove(m));
      context.SaveChanges();
      context.MenuItems.Include(m => m.Roles).ForEach(m => m.Roles.Clear());
      context.MenuItems.ForEach(m => context.MenuItems.Remove(m));
      context.SaveChanges();
      context.Pages.ForEach(m => context.Pages.Remove(m));
      context.SaveChanges();
      # endregion
      # region Assure Fresh Default Roles

      if (!context.Roles.Any()) {
        // delete all roles
        foreach (var role in Enum.GetNames(typeof (UserRole))) {
          var existingRole = context.Roles.SingleOrDefault(r => r.Name == role);
          if (existingRole != null) {
            Rolemanager.Delete(existingRole);
          }
        }
        // add default roles
        foreach (var role in Enum.GetNames(typeof (UserRole))) {
          Rolemanager.Create(new Role {
            Name = role
          });
        }
      }

      # endregion
      # region Static Portal Content
      // The static content for CMS
      foreach (string l in new string[] { "de", "en" }) {
        var doc = XDocument.Load(String.Format("PortalContent.{0}.{1}.xml", targetDeployment, l));
        var lang = new CultureInfo(l);
        LoadCmsDataForCulture(doc, app, author, lang);
      }
      # endregion
    }

    private void LoadCmsDataForCulture(XDocument doc, Application app, User author, CultureInfo lang) {
      // get the zone items
      var zones = from x in doc.Root.Elements("menu") select x;
      foreach (var zone in zones) {
        // get menus for each zone
        var menus = from z in zone.Descendants("menu").OfType<XElement>()
                    let title = z.Attribute("title").NullSafeString()
                    let roles = z.Attribute("roles").NullSafeString().Split(',').Select(r => r == "" ? UserRole.Unknown : (UserRole)Enum.Parse(typeof(UserRole), r, true))
                    let p = z.Descendants("page").OfType<XElement>().FirstOrDefault()
                    let pagetitle = p == null || p.Attribute("title") == null ? title : p.Attribute("title").NullSafeString()
                    select new CmsMenu {
                      Application = z.Attribute("application") == null ? app : GetApplication(z.Attribute("application")),
                      FeatureSet = z.Attribute("featureset") == null ? "" : z.Attribute("featureset").NullSafeString(),
                      Culture = lang,
                      Type = zone.Attribute("type").Value,
                      Title = title,
                      Description = z.Attribute("description") == null ? "Top Menu Entry" : z.Attribute("description").NullSafeString(),
                      DynamicData = z.Attribute("dynamicdata") == null ? String.Empty : z.Attribute("dynamicdata").NullSafeString(),
                      Order = z.Attribute("order") == null ? 0 : Int32.Parse(z.Attribute("order").NullSafeString()),
                      Style = z.Attribute("style") == null ? null : z.Attribute("style").NullSafeString(),
                      Page = new CmsPage {
                        Culture = lang,
                        PageContent = p == null ? String.Empty : ReadInnerXml(p),
                        PageTitle = pagetitle,
                        SeoTitle = pagetitle,
                        Status = StatusCode.Published,
                        Author = author,
                        Alias = z.Attribute("alias") == null ? "" : z.Attribute("alias").NullSafeString()
                      },
                      NavigateUrl = z.Attribute("navigateurl") == null ? null : z.Attribute("navigateurl").NullSafeString(),
                      Roles = context.Roles.ToList().Where(r => roles.Any(ro => r.UserRole == ro)).Select(r => r).ToList()
                    };
        // handle top menus
        foreach (CmsMenu menu in menus) {
          // handle menuitems and pages
          CmsMenu menutmp = menu;
          var items = from i in zone.Descendants("menuitem").OfType<XElement>()
                      where menutmp.Title.Equals(i.Parent.Attribute("title").Value)
                      let p = i.Descendants("page").OfType<XElement>().FirstOrDefault()
                      let title = i.Attribute("title") == null ? i.Value : i.Attribute("title").NullSafeString()
                      let pagetitle = p == null || p.Attribute("title") == null ? title : p.Attribute("title").NullSafeString()
                      let roles = i.Attribute("roles").NullSafeString().Split(',').Select(r => r == "" ? UserRole.Unknown : (UserRole)Enum.Parse(typeof(UserRole), r, true))
                      let dynatitle = i.Element("dynamenu") == null ? "" : i.Element("dynamenu").Attribute("title").Value
                      let dynadesc = i.Element("dynamenu") == null ? "" : i.Element("dynamenu").Attribute("description").Value
                      let dynadata = i.Element("dynamenu") == null ? "" : i.Element("dynamenu").Attribute("dynamicdata").Value
                      let dynanavi = i.Element("dynamenu") == null ? "" : i.Element("dynamenu").Attribute("navigateurl").Value
                      select new CmsMenuItem {
                        FeatureSet = i.Attribute("featureset") == null ? "" : i.Attribute("featureset").Value,
                        Application = i.Attribute("application") == null ? app : GetApplication(i.Attribute("application")),
                        Culture = lang,
                        Title = title,
                        IconClass = i.Attribute("icon") != null ? i.Attribute("icon").NullSafeString() : null,
                        Order = i.Attribute("order") == null ? 0 : Int32.Parse(i.Attribute("order").NullSafeString()),
                        NavigateUrl = i.Attribute("navigateurl") == null ? null : i.Attribute("navigateurl").NullSafeString(),
                        Description = i.Attribute("description") == null ? null : i.Attribute("description").NullSafeString(),
                        DynamicData = i.Attribute("dynamicdata") == null ? null : i.Attribute("dynamicdata").NullSafeString(),
                        Page = new CmsPage {
                          Culture = lang,
                          PageContent = p == null ? String.Empty : ReadInnerXml(p),
                          PageTitle = pagetitle,
                          SeoTitle = pagetitle,
                          Status = StatusCode.Published,
                          Author = author,
                          Alias = i.Attribute("alias") == null ? "" : i.Attribute("alias").NullSafeString()
                        },
                        DynaData = dynadata,
                        DynaDesc = dynadesc,
                        DynaTitle = dynatitle,
                        DynaNavi = dynanavi,
                        Visible = i.Attribute("visible").NullSafeBool().HasValue ? i.Attribute("visible").NullSafeBool().Value : true,
                        Roles = context.Roles.ToList().Where(r => roles.Any(ro => r.UserRole == ro)).Select(r => r).ToList()
                      };
          // add menuitems
          foreach (var item in items) {
            menu.MenuItems.Add(item);
          }
          // add menu
          context.Menus.Add(menu);
        }
      }
      context.SaveChanges();
    }

    public void LoadCatalog(Application app) {
      # region Dynamic Content
      // load categories
      Console.WriteLine(" - Load Catalog");
      var docCat = XDocument.Load("catalog\\" + TargetDeployment + "\\Catalog.xml");
      var categories = docCat.Root.Elements("OpusCategory").Select(cat => cat);
      // delete existing first TODO: (get refs, remove refs, refresh cat, restore refs)
      Action<Catalog> remCat = null;
      remCat = catalog => {
        if (catalog.HasChildren()) {
          catalog.Children.ToList().ForEach(remCat);
        }
        context.Catalog.Remove(catalog);
      };
      context.Catalog.ForEach(remCat);
      context.SaveChanges();
      // get it recursively
      LoadCategories(categories, null, app);
      var random = new Random();
      // load epub samples, texxtoor only      
      if (TargetDeployment == "T") {
        foreach (var lang in Directory.GetDirectories("epubs")) {
          if (lang.StartsWith(".")) continue;
          Console.WriteLine(" - Load EPubs from language " + lang);
          var locale = Path.GetFileName(lang);
          // get the control file
          var xml = Path.Combine(lang, "Published.xml");
          var publishedDoc = XDocument.Load(xml);
          foreach (var item in publishedDoc.Root.Elements("book")) {
            // expect the full path in the XML
            var coverPath = item.Element("coverimage").NullSafeString();
            // calc path to files (we want to acoid copying GB of test data)
            var path = Path.Combine(
              new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.FullName,
              lang);
            var file = Path.Combine(path, item.Element("filename").NullSafeString());
            // ignore if not available (not all developers need this)
            if (!File.Exists(file)) {
              Console.ForegroundColor = ConsoleColor.Cyan;
              Console.WriteLine("File {0} not captured", file);
              Console.ForegroundColor = ConsoleColor.White;
              continue;
            }
            var catalog = context.Catalog.ToList().First(c => c.Name == item.Element("catalog").NullSafeString());
            try {
              byte[] content = File.ReadAllBytes(file);
              EpubBook book = EBookFactory.Create(content);
              if (book == null) throw new NullReferenceException("Book not being processed");
              book.CoverImage = File.Exists(coverPath) ? File.ReadAllBytes(coverPath) : null;
              book.CoverDescription = String.Format("{0} of {1} published at {2}",
                book.PackageData.MetaData.Title,
                book.PackageData.MetaData.Creator,
                book.PackageData.MetaData.Publisher);
              context.Books.Add(book);
              Console.WriteLine(" - Save EPub " + file);
              context.SaveChanges();
              // the published book simulates a book as it were written be an author, based on the uploaded EPUB

              # region Create Published Work for Each EPub

              var p = new Published() {
                Title = book.PackageData.MetaData.Title.ToString(),
                SubTitle = NullSaveDcmesValue(book.PackageData.MetaData.Subject),
                Authors = context.Users.Take(2).ToList(),
                Publisher = NullSaveDcmesValue(book.PackageData.MetaData.Publisher),
                CreatedAt = File.GetCreationTime(file),
                ModifiedAt = File.GetLastAccessTime(file),
                Catalogs = new List<Catalog>(new Catalog[] { catalog }),
                Rating = Convert.ToInt32(item.Attribute("rating").NullSafeString()),
                Starred = Convert.ToInt32(item.Attribute("starred").NullSafeString())
              };
              p.CoverImage.CoverImage = File.Exists(coverPath) ? File.ReadAllBytes(coverPath) : null;
              /* Create a Published Book and add content to frozen fragment store. 
             * This not not the intended usage, we usually go from Published to Work
             * It's only for setup and testing
             * */
              // Add the content as hierarchy
              context.Published.Add(p);
              context.SaveChanges();
              var pm = new ProductionManager();
              Console.Write("<.");
              pm.ImportFromEpubSpine(book, p, context);
              Console.Write(".");
              context.SaveChanges();
              Console.Write(".>");

              # endregion

              //

              # region Marketing

              var marketing = new MarketingPackage {
                BasePrice = Convert.ToDecimal(item.Element("marketing").Attribute("baseprice").Value),
                Description = item.Element("marketing").Value,
                Name = item.Element("marketing").Attribute("name").Value,
                CreateRssFeed = true,
                AssignIsbn = false,
                CreateSocialPlatformInstances = false,
                Owner = null, // system
                RegisterForLibraries = false,
                AssignIsbnADOI = false,
                MarketingType = MarketingPackageType.PublicDomain,
                PackageBasePrice = 0
              };
              p.Marketing = marketing;
              context.MarketingPackages.Add(marketing);
              context.SaveChanges();

              # endregion

              # region Peer Reviews

              var auditings = item.Element("reviews")
                .Elements("review")
                .Select(a => new PeerReview {
                  Approved = Boolean.Parse(a.Attribute("approved").Value),
                  Comment = a.Value,
                  Level = Int32.Parse(a.Attribute("level").Value),
                  Reviewer = context.Users.ToList().First(u => u.UserName == a.Attribute("user").Value)
                })
                .ToList();
              p.Reviews.AddRange(auditings);
              context.SaveChanges();

              # endregion

              //

              # region Create Work Table After This

              // Create Public Work From Published
              var work = new Work {
                Name = p.Title,
                Note = "Generated Work",
                Owner = null // public work            
              };
              var publ = context.Published
                .Include(pu => pu.FrozenFragments)
                .First(pu => pu.Id == p.Id);
              // make working fragments from frozen fragments
              if (publ.FrozenFragments.Count(f => f.TypeOfFragment == FragmentType.Meta) != 1) {
                throw new ArgumentOutOfRangeException("publishedId");
              }
              // The metadata element is the Opus, the leading and managing element, it should be only one per Published work
              var ff = publ.FrozenFragments.Single(f => f.TypeOfFragment == FragmentType.Meta).Children;
              var wf = CopyFrozenToWorkingFragments(ff);
              work.Fragments = wf;
              context.Works.Add(work);
              context.SaveChanges();
              // assign this public work to some users
              var readers = context.Users.Where(u => u.UserName.StartsWith("Reader"));
              foreach (var user in readers) {
                work.Owner = user;
                work.Note = user.UserName + "'s work";
                context.Works.Add(work);
                break;
              }
              context.SaveChanges();

              # endregion
            } catch (Exception) {
              Debug.WriteLine("Failed: " + file);
            }
          } // en/de
        } // epub
      }
      Console.WriteLine(" - Done");
      context.SaveChanges();
      # endregion
    }

    private static List<WorkingFragment> CopyFrozenToWorkingFragments(IEnumerable<FrozenFragment> ff) {
      return ff.Select(f => new WorkingFragment {
        Children = f.HasChildren() ? CopyFrozenToWorkingFragments(f.Children) : null,
        Name = f.Name,
        OrderNr = f.OrderNr,
        FrozenFragment = f
      }).ToList();
    }


    private static string NullSaveDcmesValue(DcmesElement val) {
      if (val == null) return "";
      return val.Text;
    }

    // assume we allow HTML/XML within the <page> element
    private static string ReadInnerXml(XElement element) {
      var reader = element.CreateReader();
      reader.MoveToContent();
      return reader.ReadInnerXml();
    }

    // ReSharper disable PossibleNullReferenceException
    private void LoadRoles() {
      var xDoc = XDocument.Load("Users\\" + TargetDeployment + "\\Roles.xml");
      var query = xDoc
          .Root
          .Elements("role")
          .Select(r => new Role {
            UserRole = (UserRole)Enum.Parse(typeof(UserRole), r.Attribute("name").Value, true)
          });
      query.ForEach(role => {
        if (!context.Roles.Any(r => r.Name == role.Name)) {
          context.Roles.Add(role);
        }
      });
      context.SaveChanges();
    }

    public void LoadUsers() {
      // clean up
      context.UserProfiles.ToList().ForEach(up => context.UserProfiles.Remove(up));
      context.SaveChanges();
      context.Users.ToList().ForEach(u => context.Users.Remove(u));
      context.SaveChanges();

      // add roles
      //var xRoleDoc = XDocument.Load("Roles.xml");
      //xRoleDoc
      //  .Root
      //  .Elements("role")
      //  .ToList()
      //  .ForEach(r => Rolemanager.Create(new Role {
      //    Name = r.Attribute("name").Value            
      //  }));
      // add user
      var xUserDoc = XDocument.Load("Users\\" + TargetDeployment + "\\Users.xml");
      xUserDoc
          .Root
          .Elements("user")
          .Select(r => {
            var roles = GetRoles(r.Element("roles"), context);
            var hash = TexxtoorMembershipService.CreateHash(r.Element("membership").Element("password").Value, TexxtoorMembershipService.HashAlgorithmType);
            var user = new User() {
              UserName = r.Element("membership").Element("username").Value,
              Email = r.Element("membership").Element("email").Value,
              Password = hash,
              PasswordHash = hash,
              PasswordQuestion = r.Element("membership").Element("passwordquestion").Value,
              PasswordAnswer = r.Element("membership").Element("passwordanswer").Value,
              PasswordFormat = System.Web.Security.MembershipPasswordFormat.Hashed,
              Application = GetApplication(r.Element("application")),
              SecurityStamp = Guid.NewGuid().ToString(),
              IsLockedOut = false,
              IsApproved = true,
              // need this because the Identity system has no automatic access to these fields
              CreatedAt = DateTime.Now,
              ModifiedAt = DateTime.Now
            };
            context.Users.Add(user);
            context.SaveChanges();
            foreach (var role in roles) {
              Usermanager.AddToRole(user.Id, role.Name);
            }
            Usermanager.Update(user);
            return user;
          }).ToList();      // force execution
      // add user profiles
      var xProfiles = XDocument.Load("Users\\" + TargetDeployment + "\\UserProfiles.xml");
      var profQuery = xProfiles
        .Root
        .Elements("profile")
        .Select(p => {
          var user = GetUser(p.Element("user").Value);
          if (user != null) {
            var profile = new UserProfile() {
              User = user,
              AwardState = 1,
              Description = p.Element("description").Value,
              Walltext = p.Element("walltext").Value,
              Notes = p.Element("notes").Value,
              Status = p.GetEnumValue<AccountStatus>("status"),
              FirstName = p.Element("firstName").Value,
              LastName = p.Element("lastName").Value,
              MiddleName = p.Element("middleName").Value,
              StatementOfTaste = p.Element("statementOfTaste").Value,
              Newsletter = bool.Parse(p.Element("newsletter").Value),
              Gender = p.GetEnumValue<Gender>("gender"),
              BirthDay = String.IsNullOrEmpty(p.Element("birthDay").Value) ? DateTime.MinValue : DateTime.Parse(p.Element("birthDay").Value, CultureInfo.InvariantCulture),
              XingProfile = String.IsNullOrEmpty(p.Element("XingProfile").Value) ? null : p.Element("XingProfile").Value,
              FacebookProfile = String.IsNullOrEmpty(p.Element("FacebookProfile").Value) ? null : p.Element("FacebookProfile").Value,
              OtherProfile = String.IsNullOrEmpty(p.Element("OtherProfile").Value) ? null : p.Element("OtherProfile").Value,
              ExternalProfileUrl = null,
              Image = GetImage(p.Element("Image").Value),
              MaxHourlyRate = p.Element("maxhourlyrate") == null ? 0M : Decimal.Parse(p.Element("maxhourlyrate").Value),
              MinHourlyRate = p.Element("minhourlyrate") == null ? 0M : Decimal.Parse(p.Element("minhourlyrate").Value),
              ContractAccepted = p.Element("contractaccepted") == null || Boolean.Parse(p.Element("contractaccepted").Value),
              SharingAccepted = p.Element("sharingaccepted") == null || Boolean.Parse(p.Element("sharingaccepted").Value),
              ContributorMatrix = GetContributorRoleMatrixForUser(p.Element("contributormatrix")),
              Addresses = p.Element("addresses") == null ? null : GetAddresses(p.Element("addresses"), p.Element("firstName").Value + " " + p.Element("lastName").Value)
            };
            return profile;
          }
          return null;
        }).Where(p => p != null).ToList();
      profQuery.ForEach(profile => context.UserProfiles.Add(profile));
      context.SaveChanges();
    }

    private Application GetApplication(XObject xElement) {
      var aName = (xElement is XElement) ? ((XElement)xElement).Value : (xElement is XAttribute) ? ((XAttribute)xElement).Value : "";
      Application app;
      var apps = context.Applications.Where(a => a.ApplicationName == aName);
      if (!apps.Any()) {
        app = new Application {
          ApplicationName = aName,
          Description = "Application created for User"
        };
        context.Applications.Add(app);
        context.SaveChanges();
      } else {
        app = apps.First();
      }
      return app;
    }

    private List<ContributorMatrix> GetContributorRoleMatrixForUser(XElement e) {
      if (e == null) return null;
      var cts = new List<ContributorMatrix>();
      Console.Write(".");
      foreach (var item in e.Elements("entry")) {
        var roleName = item.Attribute("minimumrole").Value;
        var cm = new ContributorMatrix {
          ContributorRole = item.GetEnumAttribute<ContributorRole>("contributorrole"),
          Name = item.Value,
          LocaleId = item.Attribute("lang").Value,
          MinimumRole = context.Roles.Single(r => r.Name == roleName).UserRole,
          Description = item.Attribute("desc") == null ? "" : item.Attribute("desc").Value,
          Active = true
        };
        cts.Add(cm);
      }
      return cts;
    }

    private byte[] GetImage(string file, string path = "users", ImageFormat format = null) {
      if (String.IsNullOrEmpty(file)) return null;
      string fullPath = Path.Combine(path, file);
      if (!File.Exists(fullPath)) {
        fullPath = Path.Combine("users", "author.jpg");
      }
      using (var ms = new MemoryStream()) {
        var img = System.Drawing.Image.FromFile(fullPath);
        img.Save(ms, format ?? ImageFormat.Png);
        var buffer = ms.ToArray();
        return buffer;
      }
    }

    private static User GetUser(string userName) {
      // must throw exception if user does not exists!
      var query = context.Users.FirstOrDefault(u => u.UserName == userName);
      return query;
    }

    private List<AddressBook> GetAddresses(XElement addresses, string name) {
      var addList = new List<AddressBook>();
      var query = addresses.Elements("address")
          .Select(a => new AddressBook() {
            Name = name,
            StreetNumber = a.Element("street").Value,
            City = a.Element("city").Value,
            Zip = a.Element("zip").Value,
            Default = Boolean.Parse(a.Attribute("default").Value),
            Invoice = Boolean.Parse(a.Attribute("invoice").Value),
          });
      addList.AddRange(query);
      return addList;
    }
    // ReSharper restore PossibleNullReferenceException

    private List<Role> GetRoles(XElement roles, PortalContext ctx) {
      var rolesToCheck = roles.Descendants("role").ToList().Select(x => (UserRole)Enum.Parse(typeof(UserRole), x.Value, true)).ToList();
      return ctx.Roles.ToList().Where(role => rolesToCheck.Contains(role.UserRole)).ToList();
    }

    private static void LoadCategories(IEnumerable<XElement> categories, Catalog parent, Application app) {
      int order = 1;
      foreach (var categorie in categories) {
        var newCat = context.Catalog.Add(new Catalog {
          Application = app,
          Name = categorie.Descendants("name").First().Value.Trim(),
          Description = categorie.Descendants("desc").FirstOrDefault() == null ? categorie.Descendants("name").First().Value.Trim() : categorie.Descendants("desc").First().Value.Trim(),
          Culture = new CultureInfo(categorie.Attribute("lang").NullSafeString()),
          OrderNr = categorie.Attribute("order") == null ? order++ : Int32.Parse(categorie.Attribute("order").NullSafeString()) == 0 ? order++ : Int32.Parse(categorie.Attribute("order").NullSafeString()),
          Parent = parent
        });
        context.SaveChanges();
        if (categorie.Elements("OpusCategory").Any()) {
          LoadCategories(categorie.Elements("OpusCategory"), newCat, app);
        }
      }
    }


    public void AddIsbnNumbers() {
      if (!File.Exists("isbn/isbn.txt")) return;
      var isbn = new StreamReader("isbn/isbn.txt").ReadToEnd();
      isbn.Split(' ').ToList().ForEach(i => context.Isbns.Add(new IsbnStore(i)));
      context.SaveChanges();
    }

    internal void AddDemoProjects() {
      if (File.Exists("projects/projects.xml")) {
        var xProjects = XDocument.Load("projects/projects.xml");
        var root = xProjects.Root;
        var user = root.Attribute("LeadAuthor").Value;
        var tn = root.Attribute("Team").Value;
        var member = context.Users.Single(u => u.UserName == user);
        Team team = null;
        team = context.Teams.SingleOrDefault(t => t.Name == tn);
        if (team == null) {
          throw new ArgumentOutOfRangeException("Team Name");
        }
        var pr = root.Elements("Project").Select(p => new Project {
          Name = p.Attribute("Name").Value,
          Short = p.Element("Short").Value,
          Description = p.Element("Description").Value,
          IsSample = false,
          TermsAndConditions = p.Element("TermsAndConditions").Value,
          Image = GetImage(p.Element("Image").Attribute("Path").Value, "projects", ImageFormat.Png),
          Active = true,
          ApproveTerms = false,
          Marketing = new MarketingPackage() {
            Name = "Standard package",
            Description = "Standard package for generated projects",
          },
          Team = team
        });
        context.SaveChanges();
        // restore opus
        foreach (var project in pr) {
          context.Projects.Add(project);
          context.SaveChanges();
          var opuses = root.Elements("Project")
                           .Single(p => p.Attribute("Name").Value == project.Name)
                           .Element("Opuses").Elements("Opus");
          AddOpuses(opuses, project, member.UserName);
        }
        context.SaveChanges();
      }

    }

    private static void AddOpuses(IEnumerable<XElement> opuses, Project p, string userName) {
      foreach (var opusElement in opuses) {
        var file = opusElement.Attribute("Path").Value;
        var name = opusElement.Attribute("Name").Value;
        var opus = CreateOpusForProject(p, name);
        var restore = XDocument.Load(file);
        RestoreOpusFromFile(opus, restore, userName);
      }
    }

    private static Opus CreateOpusForProject(Project prj, string name) {
      var opus = new Opus {
        Name = name,
        Version = 1,
        Project = prj
      };
      context.Opuses.Add(opus);
      context.SaveChanges();
      return opus;
    }

    private static void RestoreOpusFromFile(Opus opus, XDocument xDoc, string userName) {
      var user = context.Users.Single(u => u.UserName == userName);
      var saveElements = new List<Element>();
      opus.Children.ToList().ForEach(e => SaveOpusContentRecursively(e, saveElements));
      opus.Children.ToList().ForEach(e => DeleteOpusContentRecursively(e));
      context.SaveChanges();
      Func<IEnumerable<XElement>, Element, List<Element>> helper = null;
      var currentChapter = opus.Name;
      var chapterOrder = 1;
      helper = (nodes, parent) => {
        var ret = new List<Element>();
        var orderNr = 1;
        foreach (var elm in nodes) {
          Element newElm = null;
          # region Detect Element Type
          switch (elm.Attribute("Type").Value.ToLower()) {
            case "opus":
              # region OPUS
              // do nothing as this import runs on opus level already, simply assign current as start
              opus.Name = elm.Attribute("Name").NullSafeString();
              ((Opus)opus).Version = ((Opus)opus).Version + 1;
              break;
              # endregion
            case "section":
              # region SECTION
              if (elm.FirstNode != null && elm.FirstNode.NodeType == System.Xml.XmlNodeType.Text) {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes(((XText)elm.FirstNode).Value.Trim())
                };
              } else {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes("Empty Section")
                };
              }
              if (elm.Attribute("Name") == null || String.IsNullOrEmpty(elm.Attribute("Name").Value)) {
                newElm.Name = System.Text.Encoding.UTF8.GetString(newElm.Content);
              } else {
                newElm.Name = elm.Attribute("Name").Value;
                if (elm.FirstNode == null) {
                  newElm.Content = System.Text.Encoding.UTF8.GetBytes(elm.Attribute("Name").Value);
                }
              }
              // only if import has an opus/parent part
              if (elm.Parent != null && elm.Parent.Name == "Content") {
                newElm.Parent = opus;
                currentChapter = elm.Attribute("Name").NullSafeString();
              }
              currentChapter = currentChapter ?? "Import Files";
              break;
              # endregion
            case "text":
              # region TEXT
              newElm = new TextSnippet {
                Content = Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Value.CleanUpString(15)
              };
              break;
              # endregion
            case "image":
              # region IMAGE
              var imgType = "png";
              var error = false;
              byte[] content = null;
              if (elm.Attribute("Method").NullSafeString().Equals("Base64")) {
                // assume the image is stored internally as Base64
                content = Convert.FromBase64String(elm.Value.Trim());
              } else {
                var imgpath = elm.Value.Trim();
                if (!String.IsNullOrEmpty(imgpath)) {
                  imgType = Path.GetExtension(imgpath).Substring(1); // kick the leading "."
                  try {
                    var fp = Path.Combine("/Download", imgpath);
                    if (File.Exists(fp)) {
                      content = File.ReadAllBytes(fp);
                    }
                  } catch (Exception) {
                    error = true;
                  }
                }
              }
              // volume root
              var currentChapterResFolder = context.Resources
                                               .OfType<ResourceFolder>()
                                               .SingleOrDefault(rf => rf.TypesOfResource == TypeOfResource.Content && rf.Project.Id == opus.Project.Id && rf.Parent == null);
              if (currentChapterResFolder == null) {
                // HACK: Usually the folder should be there, if not we create one on the fly, but that's not the proper way
                var localizeAttribute = typeof(TypeOfResource).GetField(TypeOfResource.Content.ToString()).GetCustomAttribute(typeof(DisplayAttribute), true) as DisplayAttribute;
                var locName = (localizeAttribute != null) ? localizeAttribute.GetName() : "Content";
                currentChapterResFolder = AddResourceFolder(opus.Project, user, locName, TypeOfResource.Content, null);
              }
              // create folder per chapter
              var currentChapterFolder = context.Resources
                                               .OfType<ResourceFolder>()
                                               .SingleOrDefault(rf => rf.Name == currentChapter && rf.Project.Id == opus.Project.Id);
              if (currentChapterFolder == null) {
                currentChapterFolder = new ResourceFolder {
                  ResourceId = Guid.NewGuid(), // we need an id even if there is no file relation to support the finder javascript (Guid == Hash)
                  Name = currentChapter,
                  Owner = user,
                  Project = opus.Project,
                  Parent = currentChapterResFolder,
                  TypesOfResource = TypeOfResource.Content,
                  OrderNr = chapterOrder++
                };
                context.Resources.Add(currentChapterFolder);
                context.SaveChanges();
              }
              //
              var res = new ResourceFile {
                Owner = user,
                Name = elm.Attribute("Name").Value,
                Project = opus.Project,
                Parent = currentChapterFolder,
                ResourceId = Guid.NewGuid(),
                TypesOfResource = TypeOfResource.Content,
                MimeType = "image/" + imgType
              };
              context.Resources.Add(res);
              var blobImg = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources);
              // TODO: Read Data and create Image 
              System.Drawing.Image img = null;
              if (content != null && content.Length > 16) {
                blobImg.Content = content;
                blobImg.Save();
                img = Image.FromStream(new MemoryStream(blobImg.Content) { Position = 0 });
              }

              // create anyway, even if there is no physical image
              newElm = new ImageSnippet {
                // images used in the active content are referenced in the blob storage but stored in the elements tree independently
                Content = content,
                Name = res.Name,
                Title = res.Name,
                MimeType = res.MimeType
              };
              var imgprops = new ImageProperties();
              if (elm.Attribute("Width") == null || elm.Attribute("Height") == null) {
                if (img != null) {
                  imgprops.ImageWidth = imgprops.OriginalWidth = img.Width;
                  imgprops.ImageHeight = imgprops.OriginalHeight = img.Height;
                } else {
                  imgprops.ImageWidth = imgprops.OriginalWidth = 100;
                  imgprops.ImageHeight = imgprops.OriginalHeight = 100;

                }
              } else {
                imgprops.ImageWidth = imgprops.OriginalWidth = Convert.ToInt32(elm.Attribute("Width").NullSafeString());
                imgprops.ImageHeight = imgprops.OriginalHeight = Convert.ToInt32(elm.Attribute("Height").NullSafeString());
              }
              ((ImageSnippet)newElm).Properties = new JavaScriptSerializer().Serialize(imgprops);
              break;
              # endregion
            case "listing":
              # region LISTING
              newElm = new ListingSnippet {
                Content = Encoding.UTF8.GetBytes(elm.Value.Trim()), //.Replace("\n", " "); - Causes problems with Listing widget. All data is displayed in one line
                Name = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Language = elm.Attribute("Language") == null ? "" : elm.Attribute("Language").Value,
                SyntaxHighlight = elm.Attribute("Highlight") == null || Boolean.Parse(elm.Attribute("Highlight").Value),
                LineNumbers = elm.Attribute("LineNumbers") == null || Boolean.Parse(elm.Attribute("LineNumbers").Value)
              };
              # endregion
              break;
            case "table":
              # region TABLE
              newElm = new TableSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                RepeatHeadRow = elm.Attribute("RepeatHeadRow") == null || Boolean.Parse(elm.Attribute("RepeatHeadRow").Value)
              };
              # endregion
              break;
            case "sidebar":
              # region SIDEBAR
              newElm = new SidebarSnippet {
                Content = Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Sidebar" : elm.Attribute("Name").Value,
                SidebarType = elm.GetEnumAttribute<SidebarType>("SidebarType")
              };
              # endregion
              break;
            default:
              throw new NotSupportedException("Unknown snippet type found in source XML: " + elm.Attribute("Type").NullSafeString());
          }
          # endregion

          // take opus as existent, add else
          if (newElm == null) {
            opus.Children = helper(elm.Elements("Element"), opus);
          } else {
            if (elm.Elements("Element").Any()) {
              newElm.Children = helper(elm.Elements("Element"), newElm);
            }
            if (parent == null) {
              throw new InvalidOperationException("Parent must exist");
            }
            newElm.OrderNr = orderNr++;
            newElm.Parent = parent;
            ret.Add(newElm);
          }
        }
        return ret;
      };
      // invoke Content loader (assume each xml contains one Opus)
      try {
        if (xDoc.Root != null) {
          var restore = helper(from o in xDoc.Root.Elements("Element") select o, opus);
          restore.ForEach(o => context.Elements.Add(o));
          context.SaveChanges();
        }
      } catch (Exception ex) {
        // Restore the old state 
        opus.Children.AddRange(saveElements);
        context.SaveChanges();
      }
    }

    private static void DeleteOpusContentRecursively(Element parentElement) {
      if (parentElement.HasChildren()) {
        foreach (var child in parentElement.Children.ToList()) {
          DeleteOpusContentRecursively(child);
        }
      }
      // if we come here we got a leaf element
      context.Elements.Remove(parentElement);
    }

    private static void SaveOpusContentRecursively(Element parentElement, List<Element> saveElements) {
      if (parentElement.HasChildren()) {
        foreach (var child in parentElement.Children.ToList()) {
          parentElement.Children.Add(child);
          SaveOpusContentRecursively(child, saveElements);
        }
      }
      // if we come here we got a leaf element
      saveElements.Add(parentElement);
    }

    private static ResourceFolder AddResourceFolder(Project project, User owner, string name, TypeOfResource type, ResourceFolder parent) {
      var folder = new ResourceFolder {
        Owner = owner,
        Name = name,
        Parent = parent, // NULL is Volume
        ResourceId = Guid.NewGuid(),
        Deleted = false,
        Private = false,
        OrderNr = 0,
        Project = project,
        TypesOfResource = type
      };
      context.Resources.Add(folder);
      context.SaveChanges();
      return folder;
    }

  }

}

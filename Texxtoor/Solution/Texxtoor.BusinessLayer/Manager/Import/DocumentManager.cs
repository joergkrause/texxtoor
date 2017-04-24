using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Texxtoor.BaseLibrary.WordInterop;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;

namespace Texxtoor.BusinessLayer {

  public class DocumentManager : Manager<DocumentManager> {

    public List<string> BuildImportResourcesTree(int projectId) {
      var res = Ctx.Resources
        .Where(r => r.Project.Id == projectId && r.TypesOfResource == TypeOfResource.Import && r.Parent == null)
        .OrderBy(r => r.Name)
        .ToList();
      var tree = new List<string>();
      BuildResourceTree(res, tree, 1);
      return tree;
    }

    private void BuildResourceTree(IEnumerable<Resource> res, List<string> tree, int indent) {
      indent++;
      foreach (var r in res) {
        if (r is ResourceFile) {
          tree.Add(String.Format("{0}", r.FullName));
        }
        if (r.HasChildren()) {
          BuildResourceTree(r.Children, tree, indent);
        }
      }
      indent--;
    }

    /// <summary>
    /// Load a saved import mapping for the given projects and imports the elements found into a new or existing opus.
    /// </summary>
    /// <param name="id">Project Id</param>
    /// <param name="opusId">Opus Id, if import goes to an existing opus.</param>
    /// <param name="importOverwrite">Overwrite existing data from a previous import.</param>
    /// <param name="splitOpus">Use this option to create a new opus for each import file.</param>
    /// <param name="MissedResults">Returns the number of missed styles (not mapped and hence not imported).</param>
    /// <param name="SuccessResults">Returns the number of imported styles (mapped and hence imported).</param>
    /// <returns>The project this import goes to.</returns>
    public Project SaveImportToProject(int id, int? opusId, bool importOverwrite, bool splitOpus, out List<string> MissedResults, out List<string> SuccessResults) {
      var prj = Ctx.Projects
        .Include("Team.Members.Role")
        .FirstOrDefault(p => p.Id == id);
      Opus opus;
      List<ResourceFile> docxRes;
      var importModule = LoadImport("Default Mapping", id);
      var successResults = new List<string>();
      var missedResults = new List<string>();
      // Attach the events to monitor the process
      importModule.GetItemFromBlobStore += new EventHandler<BlobStoreEventArgs>(importModule_GetItemFromBlobStore);
      importModule.ItemMissed += new EventHandler<ProcessEventArgs>((o, e) => missedResults.Add(e.Name));
      importModule.ItemProcessed += new EventHandler<ProcessEventArgs>((o, e) => successResults.Add(e.Name));
      importModule.StoreItemInBlobStore += new BlobStoreEventHandler(importModule_StoreItemInBlobStore);
      if (!splitOpus && opusId.HasValue) {
        opus = Ctx.Opuses.Find(opusId);
        docxRes = Ctx.Resources.Where(r => r.Project.Id == id && (r.Parent.Name == opus.Name) && r.TypesOfResource == TypeOfResource.Import && r.Name.EndsWith(".docx")).OfType<ResourceFile>().ToList();
        DoImports(id, importModule, opus, docxRes, importOverwrite);
      } else {
        // As we're creating new Opus' we need a lead for the milestones
        var owner = prj.Team.Members.First(m => m.TeamLead);
        // get all Opus and import one by one
        foreach (var item in GetImportFolders()) {
          opus = Ctx.Opuses.FirstOrDefault(o => o.Name == item && o.Project.Id == id);
          if (opus == null) {
            // opus does not exist, so we create one just right here
            opus = new Opus {
              Project = prj,
              Name = item,
              Active = true,
              Parent = null,
              Version = 1,
              PreviousVersion = null
            };
            var mstn = ProjectManager.Instance.CreateDefaultMileStones(owner, opus);
            mstn.ForEach(m => Ctx.Milestones.Add(m));
            opus.Milestones = mstn;
            Ctx.Opuses.Add(opus);
            Ctx.SaveChanges();
          }
          docxRes = Ctx.Resources.Where(r => r.Project.Id == id && (r.Parent.Name == item) && r.TypesOfResource == TypeOfResource.Import && r.Name.EndsWith(".docx")).OfType<ResourceFile>().ToList();
          DoImports(id, importModule, opus, docxRes, importOverwrite);
        }
      }
      MissedResults = missedResults;
      SuccessResults = successResults;
      return prj;
    }

    public List<string> GetImportFolders() {
      var res = Ctx.Resources
        .OfType<ResourceFolder>()
        .Where(r => r.TypesOfResource == TypeOfResource.Import && r.Parent == null)
        .Select(r => r.Name).ToList();
      return res;
    }

    private void DoImports(int id, Import importModule, Opus opus, IEnumerable<ResourceFile> docxRes, bool importOverwrite) {
      try {
        // import all files in one single step
        foreach (byte[] docx in docxRes.Select(item => BlobFactory.GetBlobStorage(item.ResourceId, BlobFactory.Container.Resources)).Select(b => b.Content)) {
          // import
          importModule.CreateHtmlFragments(docx);
        }
        // once done store fragments
        if (importModule.Fragments.Any()) {
          // get the Opus we add the content to
          var opusElement = Ctx.Elements.OfType<Opus>().FirstOrDefault(e => e.Project.Id == id && e.Id == opus.Id);
          if (importOverwrite && opusElement.HasChildren()) {
            opusElement.Children.ToList().ForEach(e => Ctx.Elements.Remove(e));
            Ctx.SaveChanges();
          }
          ReparentFragments(importModule.Fragments, opusElement);
        }
      } catch (Exception ex) {
        // handle error
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = ConsoleColor.White;
      }
    }

    private void ReparentFragments(IEnumerable<Element> fragments, Opus opusElement) {
      // determine the proposed hierarchy for chapter/sections
      Element currentParent = null;
      var ss = new Stack<Section>();
      // count the order for each level (Key = level, value = order)      
      foreach (var item in fragments) {
        if (item is Section) {
          var s = (Section)item;
          // highest level (1) if stack is empty
          Element newParent = null;
          if (ss.Count == 0) {
            ss.Push(s);
            currentParent = opusElement;
            newParent = s;
          } else {
            do {
              // if we go a level deeper just add to stack
              if (s.GetDesignatedLevelFromImport() > ss.Peek().GetDesignatedLevelFromImport()) {
                ss.Push(s);
                newParent = s;
                break;
              }
              // if same just replace with current
              if (s.GetDesignatedLevelFromImport() == ss.Peek().GetDesignatedLevelFromImport()) {
                // remove sibling
                ss.Pop();
                // take the former parent, or if nothing left, the opus
                currentParent = (ss.Count == 0) ? (Element)opusElement : ss.Peek();
                // replace 
                ss.Push(s);
                newParent = s;
                break;
              }
              // we're a level up
              newParent = ss.Pop();
              // one more?
            } while (ss.Count > 0 && s.GetDesignatedLevelFromImport() <= ss.Peek().GetDesignatedLevelFromImport());
          }
          item.Parent = currentParent;  // current parent for this section
          Element parent = currentParent;
          var siblings = Ctx.Elements.Where(e => e.Parent.Id == parent.Id);
          item.OrderNr = !siblings.Any() ? 1 : siblings.Max(e => e.OrderNr) + 1;
          Ctx.Elements.Add(item);      // add
          Ctx.SaveChanges();           // save to get the id immediately
          currentParent = newParent;    // store as new parent
          continue;                     // cont
        }
        item.Parent = currentParent;
        Ctx.Elements.Add(item);
        Ctx.SaveChanges();
      }
    }

    private Guid? importModule_StoreItemInBlobStore(object sender, BlobStoreEventArgs e) {
      return StoreBlobImage(e.ProjectId, e.Name, e.RawData);
    }

    private void importModule_GetItemFromBlobStore(object sender, BlobStoreEventArgs e) {
      e.Item = LoadBlobImage(e.ProjectId, e.Name);
    }

    private Guid? StoreBlobImage(int projectId, string fileName, Stream data) {
      var importStore = Ctx.Resources
          .Include("Owner")
          .Include("Project")
          .OfType<ResourceFolder>()
          .Single(r => r.Project.Id == projectId && r.TypesOfResource == TypeOfResource.Import && r.Parent == null);
      // Assure designated image folder in store
      ResourceFolder imgFolder = null;
      if (importStore.HasChildren()) {
        imgFolder = importStore.Children.OfType<ResourceFolder>().FirstOrDefault(r => r.Name == "Images");
      } else {
        importStore.Children = new List<Resource>();
      }
      if (imgFolder == null) {
        imgFolder = new ResourceFolder {
          Name = "Converted Images",
          Owner = importStore.Owner,
          Project = importStore.Project,
          Private = false,
          Deleted = false,
          TypesOfResource = TypeOfResource.Import,
          ResourceId = Guid.NewGuid(),
          Parent = importStore,
          Children = new List<Resource>()
        };
        Ctx.Resources.Add(imgFolder);
      }
      // Read Image to convert to JPEG
      Image img = Image.FromStream(data);
      var o = 0;
      var resFile = new ResourceFile {
        Name = System.IO.Path.GetFileNameWithoutExtension(fileName) + ".jpg",
        MimeType = "image/jpeg",
        Owner = importStore.Owner,
        Project = importStore.Project,
        Private = false,
        Deleted = false,
        OrderNr = o++,
        TypesOfResource = TypeOfResource.Import,
        ResourceId = Guid.NewGuid(),
        Parent = imgFolder
      };
      Ctx.Resources.Add(resFile);
      Ctx.SaveChanges();
      // save resources physically to blob storage
      using (var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources)) {
        using (var ms = new MemoryStream()) {
          img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
          blob.Content = ms.ToArray();
        }
        blob.Save();
      }
      return resFile.ResourceId;
    }

    private ResourceFile LoadBlobImage(int projectId, string fileName) {
      // we have always "jpg" in the blob store
      var name = System.IO.Path.GetFileNameWithoutExtension(fileName) + ".jpg";
      return Ctx.Resources.OfType<ResourceFile>().FirstOrDefault(r => r.Project.Id == projectId && r.Name == name);
    }

    public Import LoadImport(string name, int projectId) {
      var res = Ctx.Resources.OfType<ResourceFile>().SingleOrDefault(r => r.Name == name && r.Project.Id == projectId && r.MimeType == "application/x-import");
      Import import;
      if (res == null) return null;
      using (var blob = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources)) {
        if (blob.Content == null) {
          // something went wrong here, res exists but blob not, we'll trying some auto healing here
          import = new Import() { ProjectId = projectId };
          SaveImport(import, res.Id, name);
        } else {
          import = Import.Deserialize(blob.Content);
        }
      }
      return import;
    }

    public void SaveImport(Import import, int resoureceId, string newName = null) {
      var projectId = import.ProjectId;
      var prj = Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include(p => p.Team.Members.Select(m => m.Role))
        .Single(p => p.Id == projectId);
      if (!String.IsNullOrEmpty(newName)) {
        import.ImportName = newName;
      }
      var owner = prj.Team.Members.First(m => m.TeamLead);
      var res = Ctx.Resources.OfType<ResourceFile>().SingleOrDefault(r => r.Name == import.ImportName && r.Project.Id == projectId && r.MimeType == "application/x-import");
      AssureResFolder(prj, owner);
      if (res == null) {
        // first time save to DB
        res = new ResourceFile {
          ResourceId = Guid.NewGuid(),
          MimeType = "application/x-import",
          Owner = owner.Member,
          Project = prj,
          Name = import.ImportName,
          Parent = null,
          Private = true,
          Deleted = false,
          TypesOfResource = TypeOfResource.Import,
          OrderNr = 0
        };
        Ctx.Resources.Add(res);
        Ctx.SaveChanges();
      }
      // always refresh blob store
      using (var blob = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources)) {
        blob.Content = import.Serialize();
        blob.Save();
      }
      // after storing as common we assign it to the particular import file
      var importFile = Ctx.Resources.FirstOrDefault(r => r.Id == resoureceId && r.Project.Id == projectId);
      if (importFile != null) {
        using (var blob = BlobFactory.GetBlobStorage(importFile.ResourceId, BlobFactory.Container.Resources)) {
          blob.AddOrUpdateMetaData("Mapping", import);
          blob.Save();
        }
      }

    }

    private void AssureResFolder(Project prj, TeamMember owner) {
      // add missing folders in case the are not there yet
      Resource res;
      res = Ctx.Resources.FirstOrDefault(r => r.Project.Id == prj.Id && r.TypesOfResource == TypeOfResource.Content && r.Parent == null);
      if (res == null) {
        Ctx.Resources.Add(new ResourceFolder { Project = prj, ResourceId = Guid.NewGuid(), TypesOfResource = TypeOfResource.Content, Name = "Content Folder", Owner = owner.Member });
      }
      res = Ctx.Resources.FirstOrDefault(r => r.Project.Id == prj.Id && r.TypesOfResource == TypeOfResource.Import && r.Parent == null);
      if (res == null) {
        Ctx.Resources.Add(new ResourceFolder { Project = prj, ResourceId = Guid.NewGuid(), TypesOfResource = TypeOfResource.Import, Name = "Import Folder", Owner = owner.Member });
      }
      res = Ctx.Resources.FirstOrDefault(r => r.Project.Id == prj.Id && r.TypesOfResource == TypeOfResource.Project && r.Parent == null);
      if (res == null) {
        Ctx.Resources.Add(new ResourceFolder { Project = prj, ResourceId = Guid.NewGuid(), TypesOfResource = TypeOfResource.Project, Name = "Project Folder", Owner = owner.Member });
      }
      res = Ctx.Resources.FirstOrDefault(r => r.Project.Id == prj.Id && r.TypesOfResource == TypeOfResource.Trash && r.Parent == null);
      if (res == null) {
        Ctx.Resources.Add(new ResourceFolder { Project = prj, ResourceId = Guid.NewGuid(), TypesOfResource = TypeOfResource.Trash, Name = "Trash", Owner = owner.Member });
      }
      Ctx.SaveChanges();
    }

    public void AssignParaMapping(Dictionary<string, IMapObject> mapper, string styleName, string val, bool isMapped, bool split, string type) {
      if (isMapped) {
        if (!mapper.ContainsKey(styleName)) {
          int ca = 0;
          // TODO: Better level implementation, if needed
          if (styleName.StartsWith("h") && Int32.TryParse(styleName.Substring(1, 1), out ca)) {
          }
          mapper.Add(styleName, new MapObject { FragmentSplit = split, FragmentTypeName = type, ControlAttributes = ca.ToString(), ControlData = val });
        } else {
          mapper[styleName] = new MapObject { FragmentSplit = split, FragmentTypeName = type, ControlAttributes = "", ControlData = val };
        }
      } else {
        if (!mapper.ContainsKey(styleName)) {
          mapper.Add(styleName, new NoMapObject());
        } else {
          mapper[styleName] = new NoMapObject();
        }
      }
    }

    public void AssignCharMapping(Dictionary<string, IMapObject> mapper, string styleName, string val, bool isMapped, bool split = false) {
      if (isMapped) {
        if (!mapper.ContainsKey(styleName)) {
          mapper.Add(styleName, new MapObject { ControlData = val });
        } else {
          mapper[styleName] = new MapObject { ControlData = val };
        }
      } else {
        if (!mapper.ContainsKey(styleName)) {
          mapper.Add(styleName, new NoMapObject());
        } else {
          mapper[styleName] = new NoMapObject();
        }
      }
    }


    # region ************** DEMO DATA *********************

    public Dictionary<string, string> GetMappingFor(string name, string type) {
      var d = new Dictionary<string, string>();
      switch (name) {
        case "Default Mapping":
          switch (type.ToUpperInvariant()) {
            case "P":
              d.Add("div", "Map directly (no semantic meaning)");
              d.Add("h1", "Heading 1 / Chapter Level");
              d.Add("h2", "Heading 2");
              d.Add("h3", "Heading 3");
              d.Add("h4", "Heading 4");
              d.Add("h5", "Heading 5, Enum within Section");
              d.Add("h6", "Heading 6, Enum within Section");
              d.Add("p", "Paragraph");
              d.Add("ul>li", "Start Bullet Point");
              d.Add("li", "Bullet Point");
              d.Add("li<ul", "End Bullet Point");
              d.Add("aside", "Note/Tip/Caution");
              d.Add("pre", "Code Bold");
              d.Add("header", "Chapter Title");
              d.Add("figcaption", "Figure Caption");
              d.Add("code", "Code Section");
              d.Add("ol", "Numeric List");
              d.Add("caption", "Table Caption");
              d.Add("thead", "Table Head");
              d.Add("code[single]", "Single Line Code Block");
              d.Add("q", "Quote");
              break;
            case "C":
              d.Add("span", "Map text");
              d.Add("code", "Inline Code");
              break;
          }
          break;
      }
      return d;
    }

    # endregion

  }
}

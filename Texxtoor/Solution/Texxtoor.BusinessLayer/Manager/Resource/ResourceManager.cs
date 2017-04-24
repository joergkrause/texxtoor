using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.BaseLibrary.Core.Extensions;
using System.Threading.Tasks;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;

namespace Texxtoor.BusinessLayer {
  public class ResourceManager : Manager<ResourceManager> {

    public IEnumerable<ResourceFolder> GetFolderInfo(int projectId) {
      return from f in Ctx.Resources.OfType<ResourceFolder>()
             where f.Project.Id == projectId
             select f;
    }

    public ResourceFolder GetFolder(Guid resId) {
      return Ctx.Resources.OfType<ResourceFolder>().FirstOrDefault(r => r.ResourceId == resId);
    }

    public ResourceFile GetFile(Guid resId, BlobFactory.Container blobContainer = BlobFactory.Container.Resources) {
      var res = Ctx.Resources.OfType<ResourceFile>().FirstOrDefault(r => r.ResourceId == resId);
      if (res != null) {
        var blob = BlobFactory.GetBlobStorage(res.ResourceId, blobContainer);
        res.Metadata = blob.MetaData;
      }
      return res;
    }

    public ResourceFile GetFile(int id, BlobFactory.Container blobContainer = BlobFactory.Container.Resources) {
      var res = Ctx.Resources.OfType<ResourceFile>().FirstOrDefault(r => r.Id == id);
      if (res != null) {
        var blob = BlobFactory.GetBlobStorage(res.ResourceId, blobContainer);
        res.Metadata = blob.MetaData;
      }
      return res;
    }

    public byte[] GetFileData(int id, BlobFactory.Container blobContainer) {
      var res = GetFile(id);
      var blob = BlobFactory.GetBlobStorage(res.ResourceId, blobContainer);
      return blob.Content;
    }

    public ResourceFile GetFile(int id, string userName) {
      return Ctx.Resources.OfType<ResourceFile>().FirstOrDefault(r => r.Id == id && r.Owner.UserName == userName);
    }

    public Resource GetFileOrFolder(Guid resId) {
      return Ctx.Resources.FirstOrDefault(r => r.ResourceId == resId);
    }

    public ResourceFolder AddResourceFolder(Project project, string name, TypeOfResource type, ResourceFolder parent = null) {
      var owner = ProjectManager.Instance.GetTeamLeader(project.Team.Id);
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
      Ctx.Resources.Add(folder);
      SaveChanges();
      return folder;
    }

    public IEnumerable<ResourceFolder> GetVolumeFolders(int projectId, TypeOfResource volType) {
      return
        from r in Ctx.Resources.OfType<ResourceFolder>()
        where r.TypesOfResource == volType && r.Project.Id == projectId && r.Parent == null
        orderby r.Name
        select r;
    }

    public IEnumerable<ResourceFile> GetVolumeFiles(int projectId, TypeOfResource volType) {
      return
        from f in Ctx.Resources.OfType<ResourceFile>()
        where f.Parent == null && f.TypesOfResource == volType && f.Project.Id == projectId
        select f;
    }

    public IEnumerable<ResourceFile> GetFiles(ResourceFolder folder) {
      return
        from f in Ctx.Resources.OfType<ResourceFile>()
        where f.Parent.Id == folder.Id
        select f;
    }

    class FileSorter : IComparer<ResourceFile> {
      private string sortex;

      public FileSorter(string sortex) {
        this.sortex = sortex;
      }

      public int Compare(ResourceFile x, ResourceFile y) {
        switch (sortex) {
          case "OrderNr":
            return (x.OrderNr == y.OrderNr) ? 0 : x.OrderNr > y.OrderNr ? 1 : -1;
          case "Name":
            return (x.Name == y.Name) ? 0 : String.CompareOrdinal(x.Name, y.Name);
          case "FileSize":
            return (x.FileSize == y.FileSize) ? 0 : x.FileSize > y.FileSize ? 1 : -1;
        }
        return 0;
      }

      public override int GetHashCode() {
        return 0;
      }

    }

    public User GetFolderOwner(ResourceFolder folder, int teamId) {
      if (folder == null) {
        return
          Ctx.TeamMembers
             .Include("Role")
             .First(m => m.TeamLead && m.Team.Id == teamId)
             .Member;
      }
      return folder.Owner;
    }

    public TeamMember GetTeamMember(Project project, int userId) {
      return Ctx.TeamMembers
                .Include("Member")
                .Include("Team")
                .FirstOrDefault(tm => tm.Member.Id == userId && tm.Team.Id == project.Team.Id);
    }

    public void SaveResource(Resource folder, Resource element) {
      if (folder == null) {
        // Root file
        Ctx.Resources.Add(element);
      } else {
        folder.Children.Add(element);
      }
      SaveChanges();
    }

    public IEnumerable<ResourceFile> QueryFiles(string query, int projectId) {
      return
        from f in Ctx.Resources.OfType<ResourceFile>()
        where f.Name.Contains(query) && f.Project.Id == projectId
        select f;
    }

    public bool Delete(int resId, string userName) {
      var res = Ctx.Resources.Find(resId);
      if (res == null || (res.Owner != null && res.Owner.UserName != userName)) return false;
      return Delete(res.ResourceId, resId, false);
    }

    public bool Delete(Guid guid, bool removeEmptyFolder = false, BlobFactory.Container resourcesDir = BlobFactory.Container.Resources) {
      try {
        var res = Ctx.Resources.FirstOrDefault(e => e.ResourceId == guid);
        if (res == null) {
          return false;
        }
        // do not remove any deployed files
        if (Ctx.Published.Any(p => p.ResourceFiles.Any(r => r.ResourceId == guid))) {
          return false;
        }
        if (res.HasChildren()) {
          // delete children in recursive order
        } else {
          var blob = BlobFactory.GetBlobStorage(res.ResourceId, resourcesDir);
          // if already in trash, remove 
          if (res.TypesOfResource == TypeOfResource.Trash) {
            Ctx.Resources.Remove(res);
            blob.Remove();
          } else {
            // Move to trash, de-parent because trash is flat
            var p = Ctx.Projects.Find(res.Project.Id);
            res.TypesOfResource = TypeOfResource.Trash;
            res.Deleted = true;
            res.Parent = Ctx.Resources.OfType<ResourceFolder>().FirstOrDefault(r => r.Parent == null && r.TypesOfResource == TypeOfResource.Trash && r.Project.Id == p.Id);
            res.Project = p;
          }
        }
        SaveChanges();
        return true;
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        return false;
      }
    }

    /// <summary>
    /// deletes the record specifed based on id and resource id
    /// </summary>
    /// <param name="guid">Resource ID</param>
    /// <param name="resId">record ID</param>
    /// <param name="removeEmptyFolder"></param>
    /// <param name="resourcesDir"></param>
    /// <returns>Deletes the selected record</returns>
    public bool Delete(Guid guid, int resId, bool removeEmptyFolder, BlobFactory.Container resourcesDir = BlobFactory.Container.Resources) {
      try {
        var res = Ctx.Resources.FirstOrDefault(e => e.ResourceId == guid && e.Id == resId);
        if (res == null) {
          return false;
        }
        // do not remove any deployed files
        if (Ctx.Published.Any(p => p.ResourceFiles.Any(r => r.ResourceId == guid && r.Id == resId))) {
          return false;
        }
        if (res.HasChildren()) {
          // TODO: delete children in recursive order
        } else {
          // keep ref to current parent
          var parent = res.Parent;
          var blob = BlobFactory.GetBlobStorage(res.ResourceId, resourcesDir);
          // if already in trash, remove 
          if (res.TypesOfResource == TypeOfResource.Trash) {
            Ctx.Resources.Remove(res);
            blob.Remove();
          } else {
            // Move to trash, de-parent because trash is flat
            var p = Ctx.Projects.Find(res.Project.Id);
            res.TypesOfResource = TypeOfResource.Trash;
            res.Deleted = true;
            res.Parent = Ctx.Resources.OfType<ResourceFolder>().FirstOrDefault(r => r.Parent == null && r.TypesOfResource == TypeOfResource.Trash && r.Project.Id == p.Id);
            res.Project = p;
          }
          SaveChanges();
          // remove folder if there is no file in it (this is due to the relatively flat structure in file explorer)
          if (removeEmptyFolder && !parent.Children.Any()) {
            Ctx.Resources.Remove(parent);
            SaveChanges();
          }
        }
        return true;
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        return false;
      }
    }

    public Resource SetName(Guid id, string newName) {
      var res = Ctx.Resources.FirstOrDefault(r => r.ResourceId == id);
      if (res != null) {
        res.Name = newName;
      }
      SaveChanges();
      return res;
    }

    public Resource GetResourceByName(Project project, string path) {
      return Ctx.Resources
        .Where(r => r.Project.Id == project.Id)
        .ToList()
        .FirstOrDefault(r => r.FullName == path);
    }

    public User GetTeamLead(int teamId) {
      return Ctx.TeamMembers
        .Include("Role")
        .First(m => m.TeamLead && m.Team.Id == teamId)
        .Member;
    }

    public int AddResource(ResourceFile file) {
      var res = Ctx.Resources.Add(file);
      SaveChanges();
      return res.Id;
    }


    public IEnumerable<string> GetImportFolderNames(int projectId) {
      var res = Ctx.Resources.Where(r => r.Project.Id == projectId && r.TypesOfResource == TypeOfResource.Import && r.Parent == null);
      return res.OfType<ResourceFolder>().Select(r => r.Name);
    }

    public ResourceFolder GetOrAddVolumeFolder(Project startProject, TypeOfResource type, bool addVolume = false) {
      var volume = Ctx.Resources
                .Where(f => f.Project.Id == startProject.Id)
                .OfType<ResourceFolder>()
                .FirstOrDefault(f => f.TypesOfResource == type);
      // for new projects the volumes may not exist, hence we create once needed
      if (volume == null && addVolume) {
        volume = new ResourceFolder {
          TypesOfResource = type,
          Project = startProject,
          Owner = startProject.Team.TeamLead,
          OrderNr = (int)type,
          Deleted = false
        };
        volume.Name = volume.GetLocalizedTypeOfResource();
        Ctx.Resources.Add(volume);
        SaveChanges();
      }
      return volume;
    }

    public IEnumerable<ResourceFolder> GetFolders(ResourceFolder resourceFolder) {
      return Ctx.Resources
                .Single(f => f.ResourceId == resourceFolder.ResourceId)
                .Children
                .OfType<ResourceFolder>();
    }

    public ResourceFolder CreateFolder(ResourceFolder parent, string newName) {
      var newFolder = new ResourceFolder {
        Parent = parent,
        Name = newName,
        Owner = parent.Owner,
        Deleted = false,
        TypesOfResource = parent.TypesOfResource,
        ResourceId = Guid.NewGuid(),
        Private = parent.Private,
        Project = parent.Project
      };
      Ctx.Resources.Add(newFolder);
      SaveChanges();
      return newFolder;
    }

    public bool FolderExists(ResourceFolder resourceFolder, string newName) {
      return
      Ctx.Resources
         .Single(f => f.ResourceId == resourceFolder.ResourceId)
         .Children
         .Any(f => f.Name == newName);
    }

    public void RenameResource(Resource fileOrFolder, string name) {
      var res = Ctx.Resources
                   .Single(f => f.ResourceId == fileOrFolder.ResourceId);
      res.Name = name;
      SaveChanges();
    }
    public void MoveFolder(ResourceFolder src, ResourceFolder dest) {
      src.Parent = dest.Parent;
      SaveChanges();
    }
    public ResourceFile MoveFile(ResourceFile src, ResourceFolder dest) {
      src.Parent = dest;
      SaveChanges();
      return src;
    }
    public ResourceFile CopyFile(ResourceFile resourceFile, ResourceFolder resourceFolder) {
      var newResId = Guid.NewGuid();
      try {
        using (var blob = BlobFactory.GetBlobStorage(resourceFile.ResourceId, BlobFactory.Container.Resources)) {
          using (var newBlob = BlobFactory.GetBlobStorage(newResId, BlobFactory.Container.Resources)) {
            newBlob.Content = blob.Content;
            newBlob.Save();
            var newFile = new ResourceFile();
            resourceFile.CopyProperties<ResourceFile>(newFile);
            newFile.Parent = resourceFolder;
            newFile.ResourceId = newResId;
            SaveChanges();
            return newFile;
          }
        }
      } catch (Exception) {
        // something went wrong, remove file ?
        return resourceFile;
      }
    }
    public IEnumerable<Resource> GetFolderOfVolume(Project project, TypeOfResource typeOfResource) {
      return Ctx.Resources
                .Where(f => f.Project.Id == project.Id && f.TypesOfResource == typeOfResource)
                .OfType<ResourceFolder>();
    }

    public IEnumerable<ResourceFolder> GetVolumes(Project project) {
      return Ctx.Resources.Where(f => f.Project.Id == project.Id && f.Parent == null).OfType<ResourceFolder>();
    }

    public IEnumerable<ResourceFolder> GetVolumes(int projectId, string userName) {
      var p = ProjectManager.Instance.GetProject(projectId, userName);
      return GetVolumes(p);
    }

    public void EmptyFolder(Guid resId) {
      var childrenToDelete = GetFolder(resId).Children;
      childrenToDelete.ForEach(c => Delete(c.ResourceId));
      foreach (var resource in childrenToDelete) {
        using (var blob = BlobFactory.GetBlobStorage(resource.ResourceId, BlobFactory.Container.Resources)) {
          blob.Remove();
        }
      }
      SaveChanges();
    }

    public ResourceFolder AddResourceFolder(int projectId, Guid parentFolderId, string name, string userName) {
      var project = ProjectManager.Instance.GetProject(projectId, userName);
      var parent = GetFolder(parentFolderId);
      var folder = new ResourceFolder {
        Owner = parent.Owner,
        Name = name,
        Parent = parent, // NULL is Volume
        ResourceId = Guid.NewGuid(),
        Deleted = false,
        Private = false,
        OrderNr = parent.Children.Max(p => p.OrderNr) + 1,
        Project = project,
        TypesOfResource = parent.TypesOfResource
      };
      Ctx.Resources.Add(folder);
      SaveChanges();
      return folder;
    }

    public ResourceFolder AddResourceFolder(int projectId, TypeOfResource volumeType, string name, string userName) {
      var project = ProjectManager.Instance.GetProject(projectId, userName);
      var volume = GetOrAddVolumeFolder(project, volumeType);
      var owner = UserManager.Instance.GetUserByName(userName);
      var folder = new ResourceFolder {
        Owner = owner,
        Name = name,
        Parent = null, // NULL is Volume
        ResourceId = Guid.NewGuid(),
        Deleted = false,
        Private = false,
        OrderNr = !volume.Children.Any() ? 1 : volume.Children.Max(p => p.OrderNr) + 1,
        Project = project,
        TypesOfResource = volumeType
      };
      Ctx.Resources.Add(folder);
      SaveChanges();
      return folder;
    }

    public IList<ResourceFile> GetAllFiles(int projectId, TypeOfResource volumeType, string userName, params string[] mime) {
      var project = ProjectManager.Instance.GetProject(projectId, userName);
      var volume = GetOrAddVolumeFolder(project, volumeType);
      var files = volume.Children.OfType<ResourceFile>().ToList().Where(f => mime.Contains(f.MimeType)).ToList();
      Action<ResourceFolder> getFiles = null;
      getFiles = (folder) => {
        if (!folder.HasChildren()) return;
        files.AddRange(folder.Children.OfType<ResourceFile>().Where(f => mime.Contains(f.MimeType)));
        foreach (var child in folder.Children.OfType<ResourceFolder>().ToList()) {
          getFiles(child);
        }
      };
      volume.Children.OfType<ResourceFolder>().ToList().ForEach(f => getFiles(f));
      return files;
    }


    public bool RenameResource(int id, string name, string userName) {
      var res = Ctx.Resources
        .Include(r => r.Project)
        .Include(r => r.Owner)
        .SingleOrDefault(r => r.Id == id);
      if (res != null && res.Owner.UserName == userName) {
        Ctx.Entry(res).Property(r => r.Name).CurrentValue = name;
        Ctx.Entry(res).Property(r => r.Name).IsModified = true;
        return SaveChanges() >= 1;
      }
      return false;
    }

    public bool RenameResource(int id, TypeOfResource volume, string name, string label, string userName) {
      var res = Ctx.Resources
        .Include(r => r.Project)
        .Include(r => r.Owner)
        .SingleOrDefault(r => r.Id == id);
      if (res != null && res.Owner.UserName == userName) {
        Ctx.Entry(res).Property(r => r.Name).CurrentValue = name;
        Ctx.Entry(res).Property(r => r.Name).IsModified = true;
        var result = SaveChanges() >= 1;
        return MoveFile(id, volume, label, userName) || result;
      }
      return false;
    }

    public bool MoveFile(int id, TypeOfResource volume, string label, string userName) {
      var res = Ctx.Resources
        .Include(r => r.Project)
        .Include(r => r.Owner)
        .SingleOrDefault(r => r.Id == id && r.Owner.UserName == userName);
      if (res == null) return false;
      var prjId = res.Project.Id;
      ResourceFolder parent = null;
      // special treatment for moving among volumes
      if (res.TypesOfResource != volume)
      {
        res.TypesOfResource = volume;
        return SaveChanges() >= 1;
      }
      try {
        parent = Ctx.Resources.OfType<ResourceFolder>().SingleOrDefault(f => f.Name == label && f.Project.Id == prjId && f.TypesOfResource == volume);
      } catch (Exception) {
        // in case Single threw an exception we assume that we have accidentially multiple labels with same name. Hence, we make a distinct list.
        var labels = Ctx.Resources.OfType<ResourceFolder>()
          .Where(f => f.Project.Id == prjId && f.TypesOfResource == volume)   // only within volume
          .GroupBy(f => f.Name)                                               // distinct label names
          .ToList()                                                           // assume that EF can't resolve further steps, hence materialize
          .Select(g => new {                                                  // make a new temp object
            Name = g.Key,
            Folders = g.ToList(),
            Count = g.Count()
          })
          .Where(g => g.Count > 1)                                           // handle only those with > 1 label appearances
          .ToList();
        if (labels.Any()) {
          // the remaining parent
          parent = Ctx.Resources.OfType<ResourceFolder>().First(f => f.Name == label && f.Project.Id == prjId && f.TypesOfResource == volume);
          // we now have labels that appear 2 or more times only
          foreach (var l in labels) {
            var lName = l.Name;
            var files = Ctx.Resources.OfType<ResourceFile>().Where(f => f.Parent.Name == lName && f.Project.Id == prjId && f.TypesOfResource == volume);
            foreach (var resourceFile in files) {
              resourceFile.Parent = parent;
            }
          }
          // save new parents
          SaveChanges();
          // delete all labels with now entities
          var emptyLabels = Ctx.Resources.OfType<ResourceFolder>().Where(f => f.Project.Id == prjId && f.TypesOfResource == volume && !f.Children.Any());
          foreach (var resourceFolder in emptyLabels) {
            Ctx.Entry(resourceFolder).State = EntityState.Deleted;
          }
          SaveChanges();
        }
      }
      if (parent == null) {
        var user = Ctx.Users.Single(u => u.UserName == userName);
        parent = new ResourceFolder {
          TypesOfResource = volume,
          Name = label ?? String.Empty,
          Owner = user,
          OrderNr = 1,
          Private = false,
          Project = res.Project
        };
      }
      res.Parent = parent;
      res.TypesOfResource = volume;
      return SaveChanges() >= 1;
    }

    public int AddResource(int projectId, TypeOfResource type, string label, System.Web.HttpPostedFileBase file,
      string userName) {
      return AddResource(projectId, type, label, file.FileName, file.ContentType, file.InputStream.ReadToEnd(), userName);
    }

    public int AddResource(int projectId, TypeOfResource type, string label, string fileName, string fileContentType, string content, string userName) {
      return AddResource(projectId, type, label, fileName, fileContentType, Encoding.ASCII.GetBytes(content), userName);
    }

    public int AddResource(int projectId, TypeOfResource type, string label, string fileName, string fileContentType, byte[] content, string userName) {
      var project = ProjectManager.Instance.GetProject(projectId, userName);
      var user = Ctx.Users.Single(u => u.UserName == userName);
      // get label
      var parent = Ctx.Resources.OfType<ResourceFolder>().SingleOrDefault(f => f.Name == label && f.TypesOfResource == type && f.Parent == null && f.Project.Id == projectId);
      // add if not present
      if (parent == null) {
        // no folder means new label, if label provided
        parent = new ResourceFolder {
          TypesOfResource = type,
          Name = label ?? String.Empty,
          Owner = user,
          OrderNr = 1,
          Private = false,
          Project = project
        };
        // add before usage
        Ctx.Resources.Add(parent);
        SaveChanges();
      }
      var resId = Guid.NewGuid();
      ResourceFile res = null;
      using (var blob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
        try {
          blob.Content = content;
          res = new ResourceFile {
            Name = fileName,
            Parent = parent,
            Private = false,
            OrderNr = parent.Children.Any() ? parent.Children.Max(r => r.OrderNr) + 1 : 1,
            TypesOfResource = type,
            ResourceId = resId,
            Project = project,
            Owner = user,
            MimeType = fileContentType
          };
          blob.Save();
          return AddResource(res);
        } catch (Exception) {

        }
      }
      return 0;
    }


    /// <summary>
    /// Will create Duplicate record
    /// </summary>
    /// <param name="id">selected record id</param>
    /// <param name="projectId">Current Project Id</param>
    /// <param name="type">current tab</param>
    /// <param name="label">name </param>
    /// <param name="blobContent">blob</param>
    /// <param name="userName">name of the logged user</param>
    /// <param name="newName">new file name</param>
    /// <param name="resID">resource ID</param>
    /// <param name="mimetype">extension of the file</param>
    public int DuplicateResources(int id, int projectId, TypeOfResource type, string label, byte[] blobContent, string userName, string newName, Guid resID, string mimetype) {

      var project = ProjectManager.Instance.GetProject(projectId, userName);
      var volume = GetOrAddVolumeFolder(project, type);
      var parent = String.IsNullOrEmpty(label) ? volume : GetFolders(volume).SingleOrDefault(f => f.Name == label);
      if (parent == null) {
        // no folder means new label
        parent = CreateFolder(volume, label);
      }

      try {
        var resId = Guid.NewGuid();
        using (var newBlob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
          newBlob.Content = new byte[blobContent.Length];
          blobContent.CopyTo(newBlob.Content, 0);
          var res = new ResourceFile {
            Name = newName,
            Parent = parent,
            Private = false,
            OrderNr = parent.Children.Max(r => r.OrderNr) + 1,
            TypesOfResource = type,
            ResourceId = resId,
            Project = project,
            Owner = Ctx.Users.Single(u => u.UserName == userName),
            MimeType = mimetype
          };
          newBlob.Save();
          return AddResource(res);
        }
      } catch (Exception) {

      }
      return 0;
    }

    /// <summary>
    /// to get all the files in the project specified
    /// </summary>
    /// <param name="projId">project id</param>
    /// <returns>all the files for the given id</returns>
    public IEnumerable<ResourceFile> GetAllFilesByProjectId(int projId) {
      return Ctx.Resources.OfType<ResourceFile>().Where(f => f.ProjectId == projId && f.Deleted == false);
    }

    public bool UpdateResource(ResourceFile file) {
      Ctx.Entry(file).State = EntityState.Modified;
      try {
        SaveChanges();
        return true;
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        return false;
      }
    }

    public bool EmptyVolume(int projectId, TypeOfResource typeOfResource, string userName) {
      var files = Ctx.Resources.Where(r => r.Project.Id == projectId && r.TypesOfResource == typeOfResource && r.Owner.UserName == userName);
      foreach (var file in files) {
        Ctx.Resources.Remove(file);
        using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
          blob.Remove();
        }
      }
      return SaveChanges() > 0;
    }

    public void SaveResource(int id, string filename, byte[] content, string userName) {
      var res = GetFile(id, userName);
      var blob = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources);
      blob.Content = content;
      blob.Save();
      res.Name = filename;
      SaveChanges();
    }

    public void SaveResource(int id, string filename, string content, string userName) {
      SaveResource(id, filename, Encoding.ASCII.GetBytes(content), userName);
    }

    public void AddMetaDataToResource(int id, string key, object value) {
      var res = GetFile(id);
      var blob = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources);
      blob.AddOrUpdateMetaData(key, value);
      blob.Save();
    }
  }
}

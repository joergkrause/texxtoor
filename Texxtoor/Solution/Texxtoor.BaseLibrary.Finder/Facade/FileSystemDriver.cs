using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.Finder.DTO;
using Texxtoor.BaseLibrary.Finder.Response;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BaseLibrary.Finder {
  /// <summary>
  /// Represents a driver for local file system
  /// </summary>
  public class FileSystemDriver : IDriver {
    #region private
    private const string _volumePrefix = "v";
    private List<Root> _roots;
    private Project _project;

    private JsonResult Json(object data, string contentType = "application/json; charset=utf-8;") {
      return new JsonDataContractResult(data) { JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = contentType };
    }
    private void DirectoryCopy(ResourceFolder sourceDir, ResourceFolder parentdestDir, string destName, bool copySubDirs) {
      var dirs = sourceDir.Children.OfType<ResourceFolder>().ToArray();

      // If the destination directory does not exist, create it.
      var destFolder = ResourceManager.Instance.CreateFolder(parentdestDir, destName);

      // Get the file contents of the directory to copy.
      var files = sourceDir.Children.OfType<ResourceFile>().ToArray();

      foreach (var file in files) {
        // Create the path to the new copy of the file.
        var newFile = new ResourceFile();
        file.CopyProperties<ResourceFile>(newFile);
        ResourceManager.Instance.AddResource(newFile);
      }

      // If copySubDirs is true, copy the subdirectories.
      if (copySubDirs) {
        foreach (var subdir in dirs) {
          // Create the subdirectory.
          var newFolder = ResourceManager.Instance.CreateFolder(parentdestDir, subdir.Name);
          // Copy the subdirectories.
          DirectoryCopy(subdir, newFolder, destName, true);
        }
      }
    }

    #endregion

    #region public

    public FullPath ParsePath(string target){
      string volumePrefix = null;
      string pathHash = null;
      for (int i = 0; i < target.Length; i++){
        if (target[i] == '_'){
          pathHash = target.Substring(i + 1);
          volumePrefix = target.Substring(0, i + 1);
          break;
        }
      }
      Root root = _roots.First(r => r.VolumeId == volumePrefix);
      Guid resId = Guid.Empty;
      Guid.TryParse(Helper.DecodePath(pathHash), out resId);
      var resource = ResourceManager.Instance.GetFileOrFolder(resId);
      return new FullPath(root, resource);
    }

    /// <summary>
    /// Initialize new instance of class ElFinder.FileSystemDriver 
    /// </summary>
    public FileSystemDriver(Project project) {
      _roots = new List<Root>();
      _project = project;
    }

    /// <summary>
    /// Adds an object to the end of the roots.
    /// </summary>
    /// <param name="item"></param>
    public void AddRoot(Root item) {
      _roots.Add(item);
      item.VolumeId = _volumePrefix + _roots.Count + "_";
    }

    /// <summary>
    /// Gets collection of roots
    /// </summary>
    public IEnumerable<Root> Roots { get { return _roots; } }
    #endregion public

    #region   IDriver
    JsonResult IDriver.Open(string target, bool tree) {
      var fullPath = ParsePath(target);
      var answer = new OpenResponse(DTOBase.Create(fullPath.Directory, fullPath.Root), fullPath);
      if (fullPath.Directory.Parent == null) {
        // volume
        var volume = ResourceManager.Instance.GetOrAddVolumeFolder(_project, fullPath.Directory.TypesOfResource);
        if (volume != null) {
          var folders = ResourceManager.Instance.GetFolders(fullPath.Directory);
          foreach (var item in folders.Where(item => item.CanRead())){
            answer.Files.Add(DTOBase.Create(item, fullPath.Root));
          }
          // if volume does not exists we treat it as empty
          var files = ResourceManager.Instance.GetFiles(fullPath.Directory);
          foreach (var item in files.Where(item => item.CanRead())){
            answer.Files.Add(DTOBase.Create(item, fullPath.Root));
          }
        }
      } else {
        var folders = ResourceManager.Instance.GetFolders(fullPath.Directory);
        foreach (var item in folders.Where(item => item.CanRead())){
          answer.Files.Add(DTOBase.Create(item, fullPath.Root));
        }
        var files = ResourceManager.Instance.GetFiles(fullPath.Directory);
        foreach (var item in files) {
          //sfullPath.Root.Directory = fullPath.Directory;
          if (item.CanRead())
            answer.Files.Add(DTOBase.Create(item, fullPath.Root));
        }
      }
      return Json(answer);
    }

    JsonResult IDriver.Init(string target) {
      FullPath fullPath;
      if (string.IsNullOrEmpty(target)) {
        var root = _roots.FirstOrDefault(r => r.StartPath != null);
        if (root == null)
          root = _roots.First();
        fullPath = new FullPath(root, root.Directory);
      } else {
        fullPath = ParsePath(target);
      }
      var answer = new InitResponse(DTOBase.Create(fullPath.Directory, fullPath.Root), new Options(fullPath));

      foreach (var item in _roots) {
        answer.Files.Add(DTOBase.Create(item.Directory, item));
      }
      if (fullPath.Root.Directory.FullName != fullPath.Directory.FullName) {
        foreach (var item in ResourceManager.Instance.GetFiles(fullPath.Directory).Where(item => item.CanRead())) {
          answer.Files.Add(DTOBase.Create(item, fullPath.Root));
        }
      }
      else {
        foreach (var item in ResourceManager.Instance.GetFiles(fullPath.Directory).Where(item => item.CanRead())) {
          fullPath.Root.Directory = fullPath.Directory;
          answer.Files.Add(DTOBase.Create(item, fullPath.Root));
        }
      }
      foreach (var item in ResourceManager.Instance.GetFolders(fullPath.Directory).Where(item => item.CanRead())) {
        //sfullPath.Root.Directory = fullPath.Directory;
        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
      }

      if (fullPath.Root.MaxUploadSize.HasValue) {
        answer.UploadMaxSize = fullPath.Root.MaxUploadSizeInKb.Value + "K";
      }
      return Json(answer);
    }
    ActionResult IDriver.File(string target, bool download) {
      var fullPath = ParsePath(target);
      if (fullPath.IsDirectory)
        return new HttpStatusCodeResult(403, "You can not download whole folder");
      if (fullPath.Root.IsShowOnly)
        return new HttpStatusCodeResult(403, "Access denied. Volume is for show only");
      return new DownloadFileResult(fullPath.File, download);
    }
    JsonResult IDriver.Parents(string target) {
      var fullPath = ParsePath(target);
      var answer = new TreeResponse();
      if (fullPath.Directory.FullName == fullPath.Root.Directory.FullName) {
        answer.Tree.Add(DTOBase.Create(fullPath.Directory, fullPath.Root));
      } else {
        var parent = fullPath.Directory;
        foreach (var item in ResourceManager.Instance.GetFolders(parent)) {
          answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
        }
        while (parent != null && parent.FullName != fullPath.Root.Directory.FullName) {
          parent = parent.Parent as ResourceFolder;
          answer.Tree.Add(DTOBase.Create(parent, fullPath.Root));
        }
      }
      return Json(answer);
    }
    JsonResult IDriver.Tree(string target) {
      var fullPath = ParsePath(target);
      var answer = new TreeResponse();
      if (fullPath.Directory.ResourceId.Equals(Guid.Empty)) {
        // Volumen path
        var volume = ResourceManager.Instance.GetOrAddVolumeFolder(_project, fullPath.Directory.TypesOfResource);
        if (volume != null) {
          foreach (var item in volume.Children.OfType<ResourceFolder>()) {
            answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
          }
        }
      } else {
        foreach (var item in ResourceManager.Instance.GetFolders(fullPath.Directory)) {
          answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
        }
      }
      return Json(answer);
    }
    JsonResult IDriver.List(string target) {
      var fullPath = ParsePath(target);
      var answer = new ListResponse();
      foreach (var item in ResourceManager.Instance.GetFiles(fullPath.Directory)) {
        answer.List.Add(item.Name);
      }
      return Json(answer);
    }
    JsonResult IDriver.MakeDir(string target, string name) {
      var fullPath = ParsePath(target);
      var newDir = ResourceManager.Instance.AddResourceFolder(_project, name, fullPath.Directory.TypesOfResource, fullPath.Directory);
      return Json(new AddResponse(newDir, fullPath.Root));
    }
    JsonResult IDriver.MakeFile(string target, string name) {
      var fullPath = ParsePath(target);

      var newFile = new ResourceFile {
        Name = name,
        Parent = fullPath.Directory,
        MimeType = Helper.GetMimeType(name),
        Owner = fullPath.Directory.Owner,
        TypesOfResource = fullPath.Directory.TypesOfResource,
        ResourceId = Guid.NewGuid(),
        Private = fullPath.Directory.Private,
        Project = fullPath.Directory.Project
      };
      using (var blob = BlobFactory.GetBlobStorage(newFile.ResourceId, BlobFactory.Container.Resources)) {
        blob.Save();
      }
      ResourceManager.Instance.AddResource(newFile);
      return Json(new AddResponse(newFile, fullPath.Root));
    }
    JsonResult IDriver.Rename(string target, string name) {
      var fullPath = ParsePath(target);
      var answer = new ReplaceResponse();
      answer.Removed.Add(target);
      if (fullPath.Directory != null) {
        var folder = ResourceManager.Instance.GetFolder(fullPath.Directory.ResourceId);
        ResourceManager.Instance.RenameResource(folder, name);
        answer.Added.Add(DTOBase.Create(folder, fullPath.Root));
      } else {
        var file = ResourceManager.Instance.GetFile(fullPath.File.ResourceId);
        ResourceManager.Instance.RenameResource(file, name);
        answer.Added.Add(DTOBase.Create(new ResourceFile(), fullPath.Root));
      }
      return Json(answer);
    }
    JsonResult IDriver.Remove(IEnumerable<string> targets) {
      var answer = new RemoveResponse();
      foreach (var item in targets) {
        var fullPath = ParsePath(item);
        if (fullPath.Directory != null) {
          ResourceManager.Instance.Delete(fullPath.Directory.ResourceId);
        } else {
          ResourceManager.Instance.Delete(fullPath.File.ResourceId);
        }
        answer.Removed.Add(item);
      }
      return Json(answer);
    }
    JsonResult IDriver.Get(string target) {
      var fullPath = ParsePath(target);
      var answer = new GetResponse();
      using (var blob = BlobFactory.GetBlobStorage(fullPath.File.ResourceId, BlobFactory.Container.Resources)) {
        answer.Content = Encoding.ASCII.GetString(blob.Content);
      }
      return Json(answer);
    }
    JsonResult IDriver.Put(string target, string content) {
      var fullPath = ParsePath(target);
      var answer = new ChangedResponse();
      using (var blob = BlobFactory.GetBlobStorage(fullPath.File.ResourceId, BlobFactory.Container.Resources)) {
        blob.Content = Encoding.ASCII.GetBytes(content);
        blob.Save();
      }
      answer.Changed.Add((FileDTO)DTOBase.Create(fullPath.File, fullPath.Root));
      return Json(answer);
    }
    JsonResult IDriver.Paste(string source, string dest, IEnumerable<string> targets, bool isCut) {
      var destPath = ParsePath(dest);
      var response = new ReplaceResponse();
      foreach (var item in targets) {
        var src = ParsePath(item);
        if (src.Directory != null) {
          if (isCut) {
            ResourceManager.Instance.MoveFolder(src.Directory, destPath.Directory);
            response.Removed.Add(item);
          } else {
            DirectoryCopy(src.Directory, destPath.Directory, item, true);
          }
          response.Added.Add(DTOBase.Create(destPath.Directory, destPath.Root));
        } else {
          ResourceFile newFile;
          if (isCut) {
            newFile = ResourceManager.Instance.MoveFile(src.File, destPath.Directory);
            response.Removed.Add(item);
          } else {
            newFile = ResourceManager.Instance.CopyFile(src.File, destPath.Directory);
          }
          response.Added.Add(DTOBase.Create(newFile, destPath.Root));
        }
      }
      return Json(response);
    }
    JsonResult IDriver.Upload(string target, System.Web.HttpFileCollectionBase targets) {
      var dest = ParsePath(target);
      var response = new AddResponse();
      if (dest.Root.MaxUploadSize.HasValue) {
        for (int i = 0; i < targets.AllKeys.Length; i++) {
          HttpPostedFileBase file = targets[i];
          if (file.ContentLength > dest.Root.MaxUploadSize.Value) {
            return Error.MaxUploadFileSize();
          }
        }
      }
      for (int i = 0; i < targets.AllKeys.Length; i++) {
        var file = targets[i];
        var newFile = new ResourceFile {
          Name = file.FileName,
          Parent = dest.Directory,
          Owner = dest.Directory.Owner,
          ResourceId = Guid.NewGuid(),
          Private = dest.Directory.Private,
          TypesOfResource = dest.Directory.TypesOfResource,
          Project = dest.Directory.Project,
          MimeType = Helper.GetMimeType(file.FileName)
        };
        using (var blob = BlobFactory.GetBlobStorage(newFile.ResourceId, BlobFactory.Container.Resources)) {
          if (dest.Root.UploadOverwrite) {
            //if file already exist we rename the current file, 
            //and if upload is succesfully delete temp file, in otherwise we restore old file
            var uploaded = false;
            try {
              var bytes = new byte[file.InputStream.Length];
              file.InputStream.Seek(0, SeekOrigin.Begin);
              file.InputStream.Read(bytes, 0, bytes.Length);
              blob.Content = bytes;
              blob.Save();
              uploaded = true;
            } catch {
            } finally {
              if (!uploaded) {
                blob.Remove();
              }
            }
          } else {
            throw new NotImplementedException("");
          }
          ResourceManager.Instance.AddResource(newFile);
          response.Added.Add((FileDTO)DTOBase.Create(newFile, dest.Root));
        }
      }
      if (!HttpContext.Current.Request.AcceptTypes.Contains("application/json"))
        return Json(response, "text/html");
      else
        return Json(response);
    }
    JsonResult IDriver.Duplicate(IEnumerable<string> targets) {
      var response = new AddResponse();
      foreach (var target in targets) {
        var fullPath = ParsePath(target);
        if (fullPath.Directory != null) {
          var parentPath = fullPath.Directory.Parent.FullName;
          var parentFolder = fullPath.Directory.Parent as ResourceFolder;
          var name = fullPath.Directory.Name;
          var newName = string.Format(@"{0}\{1} copy", parentPath, name);
          if (!ResourceManager.Instance.FolderExists(fullPath.Directory, newName)) {
            DirectoryCopy(fullPath.Directory, parentFolder, newName, true);
          } else {
            for (int i = 1; i < 100; i++) {
              newName = string.Format(@"{0}\{1} copy {2}", parentPath, name, i);
              if (!Directory.Exists(newName)) {
                DirectoryCopy(fullPath.Directory, parentFolder, newName, true);
                break;
              }
            }
          }
          var newFolder = ResourceManager.Instance.CreateFolder(parentFolder, newName);
          response.Added.Add(DTOBase.Create(newFolder, fullPath.Root));
        } else {
          ResourceFile copiedFile = null;
          var parentPath = fullPath.File.Parent.FullName;
          var parentFolder = fullPath.File.Parent as ResourceFolder;
          var name = fullPath.File.Name.Substring(0, fullPath.File.Name.LastIndexOf("."));
          var ext = fullPath.File.Name.Substring(fullPath.File.Name.LastIndexOf("."));

          var newName = string.Format(@"{0}\{1} copy{2}", parentPath, name, ext);

          if (ResourceManager.Instance.GetFiles(parentFolder).Any(f => f.Name == name)) {
            for (int i = 1; i < 100; i++) {
              newName = string.Format(@"{0}\{1} copy {2}{3}", parentPath, name, i, ext);
              if (ResourceManager.Instance.GetFiles(parentFolder).All(f => f.Name != newName)) {
                copiedFile = ResourceManager.Instance.CopyFile(fullPath.File, parentFolder);
                break;
              }
            }
          } else {
            copiedFile = ResourceManager.Instance.CopyFile(fullPath.File, parentFolder);
          }
          response.Added.Add(DTOBase.Create(copiedFile, fullPath.Root));
        }
      }
      return Json(response);
    }
    JsonResult IDriver.Thumbs(IEnumerable<string> targets) {
      var response = new ThumbsResponse();
      foreach (var target in targets) {
        var path = ParsePath(target);
        response.Images.Add(target, String.Format("/Tools/GetThumbnail/", path.File));
      }
      return Json(response);
    }
    JsonResult IDriver.Dim(string target) {
      var path = ParsePath(target);
      var response = new DimResponse(path.Root.GetImageDimension(path.File));
      return Json(response);
    }
    JsonResult IDriver.Resize(string target, int width, int height) {
      FullPath path = ParsePath(target);
      path.Root.PicturesEditor.Resize(path.File.FullName, width, height);
      var output = new ChangedResponse();
      output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
      return Json(output);
    }
    JsonResult IDriver.Crop(string target, int x, int y, int width, int height) {
      var path = ParsePath(target);
      path.Root.PicturesEditor.Crop(path.File.FullName, x, y, width, height);
      var output = new ChangedResponse();
      output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
      return Json(output);
    }
    JsonResult IDriver.Rotate(string target, int degree) {
      FullPath path = ParsePath(target);
      path.Root.PicturesEditor.Rotate(path.File.FullName, degree);
      var output = new ChangedResponse();
      output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
      return Json(output);
    }

    #endregion IDriver
  }
}
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.Finder;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BusinessLayer.Finder.DTO {
  [DataContract]
  internal abstract class DTOBase {
    protected static readonly DateTime _unixOrigin = new DateTime(1970, 1, 1, 0, 0, 0);

    /// <summary>
    ///  Name of file/dir. Required
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; protected set; }

    /// <summary>
    ///  Hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required.
    /// </summary>
    [DataMember(Name = "hash")]
    public string Hash { get; protected set; }

    /// <summary>
    ///  mime type. Required.
    /// </summary>
    [DataMember(Name = "mime")]
    public string Mime { get; protected set; }

    /// <summary>
    /// file modification time in unix timestamp. Required.
    /// </summary>
    [DataMember(Name = "ts")]
    public long UnixTimeStamp { get; protected set; }

    /// <summary>
    ///  file size in bytes
    /// </summary>
    [DataMember(Name = "size")]
    public long Size { get; protected set; }

    /// <summary>
    ///  is readable
    /// </summary>
    [DataMember(Name = "read")]
    public byte Read { get; protected set; }

    /// <summary>
    /// is writable
    /// </summary>
    [DataMember(Name = "write")]
    public byte Write { get; protected set; }

    /// <summary>
    ///  is file locked. If locked that object cannot be deleted and renamed
    /// </summary>
    [DataMember(Name = "locked")]
    public byte Locked { get; protected set; }

    public static DTOBase Create(ResourceFile info, Root root) {
      if (info == null)
        throw new ArgumentNullException("info");
      if (root == null)
        throw new ArgumentNullException("root");
      var parent = info.Parent;
      using (var blob = BlobFactory.GetBlobStorage(info.ResourceId, BlobFactory.Container.Resources)) {
        var response = new FileDTO {
          Read = 1,
          Write = root.IsReadOnly ? (byte)0 : (byte)1,
          Locked = root.IsLocked ? (byte)1 : (byte)0,
          Name = info.Name,
          Size = blob.Content == null ? 0 : blob.Content.Length,
          UnixTimeStamp = (long)(info.ModifiedAt - _unixOrigin).TotalSeconds,
          Mime = Helper.GetMimeType(info.Name) ?? info.MimeType,
          Hash = root.VolumeId + Helper.EncodePath(info.ResourceId),
          ParentHash = root.VolumeId +
                       Helper.EncodePath(parent != null ? parent.ResourceId : root.Directory.ResourceId),
          ThumbnailId = info.Id
        };
        return response;

      }
    }

    public static DTOBase Create(ResourceFolder directory, Root root) {
      if (directory == null)
        throw new ArgumentNullException("directory");
      if (root == null)
        throw new ArgumentNullException("root");
      if (root.Directory.FullName == directory.FullName) {
        var response = new RootDTO() {
          Mime = "directory",
          Dirs = directory.Children.OfType<ResourceFolder>().Any() ? (byte)1 : (byte)0,
          Hash = root.VolumeId + Helper.EncodePath(directory.ResourceId),
          Read = 1,
          Write = root.IsReadOnly ? (byte)0 : (byte)1,
          Locked = root.IsLocked ? (byte)1 : (byte)0,
          Name = root.Alias,
          Size = 0,
          UnixTimeStamp = (long)(directory.ModifiedAt - _unixOrigin).TotalSeconds,
          VolumeId = root.VolumeId
        };
        return response;
      } else {
        var parentId = directory.Parent == null ? Guid.Empty : directory.Parent.ResourceId;
        var response = new DirectoryDTO() {
          Mime = "directory",
          ContainsChildDirs = directory.Children.OfType<ResourceFolder>().Any() ? (byte)1 : (byte)0,
          Hash = root.VolumeId + Helper.EncodePath(directory.ResourceId),
          Read = 1,
          Write = root.IsReadOnly ? (byte)0 : (byte)1,
          Locked = root.IsLocked ? (byte)1 : (byte)0,
          Size = 0,
          Name = directory.Name,
          UnixTimeStamp = (long)(directory.ModifiedAt - _unixOrigin).TotalSeconds,
          ParentHash = root.VolumeId + Helper.EncodePath(parentId)
        };
        return response;
      }
    }

  }
}
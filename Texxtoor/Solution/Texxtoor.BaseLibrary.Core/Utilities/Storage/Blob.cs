using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.Web.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Xml;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {

  /// <summary>
  /// A BLOB storage class that abstracts file based operation from application.
  /// </summary>
  /// <remarks>
  /// Locally this need the emulator:
  /// C:\Program Files\Microsoft SDKs\Windows Azure\Emulator\csmonitor.exe ==> Start Storage Emulator, Show UI
  /// </remarks>
  public class Blob : IBlob {

    private const string DataExtension = "dat";
    private byte[] _content;
    private IDictionary<string, string> _metaData;
    private readonly string _dataFile;
    private bool _contentDirty;
    private bool _metaDirty;

    # region Azure Storage

    private readonly CloudStorageAccount _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString);
    private readonly CloudBlobContainer _blobContainer;

    # endregion

    public Blob(string container, Guid id) {
      Id = id;
      container = container.ToLowerInvariant(); // http://msdn.microsoft.com/library/azure/dd135715.aspx : 3. All letters in a container name must be lowercase.
      CloudBlobClient blobStorage = _storageAccount.CreateCloudBlobClient();
      _blobContainer = blobStorage.GetContainerReference(container);
      if (_blobContainer.CreateIfNotExists()) {
        var permissions = _blobContainer.GetPermissions();
        permissions.PublicAccess = BlobContainerPublicAccessType.Container;
        _blobContainer.SetPermissions(permissions);
      }
      _dataFile = String.Format("{0}.{1}", Id, DataExtension);
    }

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    /// <value>The id.</value>
    public Guid Id { get; set; }

    public IDictionary<string, object> MetaData {
      get {
        return MetaDataStore.ToDictionary(k => k.Key, v => DeSerializeObject(v.Value));
      }
    }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public byte[] Content {
      get {
        if (_content == null) {
          using (var ms = new MemoryStream()) {
            try {
              _blobContainer.GetBlockBlobReference(_dataFile).DownloadToStream(ms);
              _content = ms.ToArray();
            } catch (Exception) {
              _content = null;
            }
          }
        }
        return _content;
      }
      set {
        _contentDirty = true;
        _content = value;
      }
    }

    public Uri BlobUri {
      get { return _blobContainer.GetBlockBlobReference(_dataFile).Uri; }
    }

    /// <summary>
    /// Gets the meta data.
    /// </summary>
    /// <value>The meta data.</value>
    internal IDictionary<string, string> MetaDataStore {
      get {
        if (_metaData != null) return _metaData;
        _blobContainer.FetchAttributes();
        if (_blobContainer.Metadata.Any()) {
          _metaData = _blobContainer.Metadata;
        } else {
          _metaData = new Dictionary<string, string>();
        }
        return _metaData;
      }
    }

    private static string SerializeObject(object value) {
      return Convert.ToBase64String(StorageSerializer.Serialize(value));
    }

    private static object DeSerializeObject(string value) {
      return StorageSerializer.Deserialize(Convert.FromBase64String(value));
    }


    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> at the specified index.
    /// </summary>
    /// <value></value>
    public object this[string index] {
      get { return MetaDataStore.ContainsKey(index) ? DeSerializeObject(MetaDataStore[index]) : null; }
      set {
        if (MetaDataStore.ContainsKey(index)) {
          if (value != null) {
            MetaDataStore[index] = SerializeObject(value);
          } else {
            MetaDataStore.Remove(index);
          }
        } else {
          MetaDataStore.Add(index, SerializeObject(value));
        }
        _metaDirty = true;
      }
    }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public void Save() {
      if (_contentDirty) {
        using (var ms = new MemoryStream(_content)) {
          _blobContainer.GetBlockBlobReference(_dataFile).UploadFromStream(ms);
        }
        _contentDirty = false;
      }
      if (_metaDirty && MetaDataStore.Any()) {
        var metaDataCopy = new Dictionary<string, string>(MetaDataStore);
        _blobContainer.Metadata.Clear();
        foreach (var md in metaDataCopy) {
          _blobContainer.Metadata.Add(md.Key, md.Value);
        }
        _blobContainer.SetMetadata();
      }
      _metaDirty = false;
    }

    public bool Remove() {
      try {
        _blobContainer.Metadata.Clear();     
        _blobContainer.GetBlockBlobReference(_dataFile).Delete();
        _blobContainer.Delete();
        return true;
      } catch (Exception) {
        return false;
      }
    }

    public void Dispose() {
      Content = null;
    }

    public void AddOrUpdateMetaData(string key, object store) {
      if (_metaData == null) {
        // first, try to fetch existing
        _blobContainer.FetchAttributes();
        _metaData = _blobContainer.Metadata;
      }
      // if still nothing create an empty container
      if (_metaData == null) _metaData = new Dictionary<string, string>();
      // update or insert
      if (_metaData.ContainsKey(key)) {
        _metaData[key] = SerializeObject(store);
        _metaDirty = true;
      } else {
        _metaData.Add(key, SerializeObject(store));
        _metaDirty = true;
      }
    }



    public void Save(Action callback) {
      try {
        Save();
        callback();
      } catch (Exception) {
      }
    }
  }
}
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Configuration;
using System.Web.Configuration;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {

  /// <summary>
  /// A BLOB storage class that abstracts file based operation from application.
  /// </summary>
  public class FileBlob : IBlob {

    private const string DataExtension = "dat";
    private const string MetaExtension = "xml";
    private byte[] _content;
    private SerializableDictionary<string, object> _metaData;
    private readonly string _dataFile;
    private readonly string _metaFile;
    private bool _contentDirty;
    private bool _metaDirty;


    public FileBlob(Guid id)
      : this("Resources", id) {
    }

    public FileBlob(string container, Guid id) {
      Id = id;
      EnsureDirectory(Path.Combine(BlobDataDir, container));
      _dataFile = Path.Combine(BlobDataDir, container, string.Format("{0}.{1}", Id, DataExtension));
      _metaFile = Path.Combine(BlobDataDir, container, string.Format("{0}.{1}", Id, MetaExtension));
    }

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    /// <value>The id.</value>
    public Guid Id { get; set; }

    public string DataFilePath { get { return _dataFile; } }
    public string MetaFilePath { get { return _metaFile; } }

    public IDictionary<string, object> MetaData {
      get {
        return MetaDataStore;
      }
    }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public byte[] Content {
      get {
        if (_content == null && File.Exists(_dataFile)) {
          using (var stream = new FileStream(_dataFile, FileMode.Open)) {
            _content = new byte[stream.Length];
            stream.Read(_content, 0, (int)stream.Length);
            stream.Close();
          }
        }
        return _content;
      }
      set {
        _contentDirty = true;
        _content = value;
      }
    }

    /// <summary>
    /// Gets the meta data.
    /// </summary>
    /// <value>The meta data.</value>
    internal SerializableDictionary<string, object> MetaDataStore {
      get {
        if (_metaData != null) return _metaData;
        if (File.Exists(_metaFile)) {
          var serializer = new XmlSerializer(typeof(SerializableDictionary<string, object>));
          var reader = new StreamReader(_metaFile, Encoding.UTF8);
          try {
            _metaData = (SerializableDictionary<string, object>)(serializer.Deserialize(reader));
          } catch (Exception) {
            _metaData = new SerializableDictionary<string, object>();
          }
          reader.Close();
        } else {
          _metaData = new SerializableDictionary<string, object>();
        }
        return _metaData;
      }
    }


    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> at the specified index.
    /// </summary>
    /// <value></value>
    public object this[string index] {
      get { return MetaDataStore.ContainsKey(index) ? MetaDataStore[index] : null; }
      set {
        if (MetaDataStore.ContainsKey(index)) {
          if (value != null) {
            MetaDataStore[index] = value;
          } else {
            MetaDataStore.Remove(index);
          }
        } else {
          MetaDataStore.Add(index, value);
        }
        _metaDirty = true;
      }
    }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public void Save() {
      if (_contentDirty) {
        using (var stream = new FileStream(_dataFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
          stream.Write(Content, 0, Content.Length);
          stream.Close();
        }
        _contentDirty = false;
      }
      if (_metaDirty) {
        if (MetaDataStore.Count > 0) {
          var serializer = new XmlSerializer(typeof(SerializableDictionary<string, object>));
          var writer = new StreamWriter(_metaFile);
          serializer.Serialize(writer, MetaDataStore);
          writer.Close();
        } else if (File.Exists(_metaFile)) {
          File.Delete(_metaFile);
        }
        _metaDirty = false;
      }
    }

    public bool Remove() {
      bool success = false;
      if (File.Exists(_metaFile)) {
        File.Delete(_metaFile);
        success = true;
      }
      if (File.Exists(_dataFile)) {
        File.Delete(_dataFile); success = true;

      }
      return success;
    }

    #region -= Enumeration =-
    // ReSharper disable AssignNullToNotNullAttribute
    public static IEnumerable<Blob> GetAll(string container) {
      return Directory.EnumerateFiles(Path.Combine(BlobDataDir, container), "*.dat")
        .Select(d => new Blob(container, new Guid(Path.GetFileNameWithoutExtension(d))));
    }
    // ReSharper restore AssignNullToNotNullAttribute

    #endregion

    # region -= Ensure Data Dir =-
    /// <summary>
    /// 
    /// </summary>
    private static readonly string BlobDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Blobs");

    private static void EnsureDirectory(string dir) {
      if (!Directory.Exists(dir)) {
        Directory.CreateDirectory(dir);
      }
    }

    static FileBlob() {
      EnsureDirectory(BlobDataDir);
    }
    #endregion

    public void Dispose() {
      Content = null;
    }

    public void AddOrUpdateMetaData(string key, object store) {
      if (_metaData.ContainsKey(key)) {
        _metaData[key] = store;
      } else {
        _metaData.Add(key, store);
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
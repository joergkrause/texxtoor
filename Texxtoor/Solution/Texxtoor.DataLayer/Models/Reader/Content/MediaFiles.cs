using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// Helper to keep all media related files in blob store.
  /// </summary>
  [Serializable]
  public class MediaFiles : ICollection<MediaFile> {

    public MediaFiles() {
      Files = new List<MediaFile>();
    }

    private List<MediaFile> Files { get; set; }

    public static MediaFiles Deserialize(string data) {
      var xs = new XmlSerializer(typeof(MediaFiles));
      using (var sr = new StringReader(data)) {
        return xs.Deserialize(sr) as MediaFiles;
      }
    }

    public override string ToString() {
      var xs = new XmlSerializer(typeof(MediaFiles));
      var sb = new StringBuilder();
      using (var sw = new StringWriter(sb)) {
        xs.Serialize(sw, this);
        return sb.ToString();
      }
    }

    public void Add(MediaFile item) {
      Files.Add(item);
    }

    public void Clear() {
      Files.Clear();
    }

    public bool Contains(MediaFile item) {
      return Files.Contains(item);
    }

    public void CopyTo(MediaFile[] array, int arrayIndex) {
      throw new NotImplementedException();
    }

    public int Count {
      get { return Files.Count; }
    }

    public bool Remove(MediaFile item) {
      return Files.Remove(item);
    }

    public IEnumerator<MediaFile> GetEnumerator() {
      return Files.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return Files.GetEnumerator();
    }


    public bool IsReadOnly {
      get { return false; }
    }
  }


  [Serializable]
  public class MediaFile {


    public MediaFile(Guid file, string media) {
      FileGuid = file;
      Media = media;
    }

    /// <summary>
    /// Guid in Blob Store
    /// </summary>
    public Guid FileGuid { get; set; }

    /// <summary>
    /// Related media type name, such as "Web", "Epub", "PDF"
    /// </summary>
    public string Media { get; set; }

  }
}
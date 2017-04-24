using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {
  public interface IBlob : IDisposable {
    IDictionary<string, object> MetaData { get; }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    byte[] Content { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> at the specified index.
    /// </summary>
    /// <value></value>
    object this[string index] { get; set; }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    void Save();

    void Save(Action callback);

    bool Remove();

    void AddOrUpdateMetaData(string key, object store);
  }
}
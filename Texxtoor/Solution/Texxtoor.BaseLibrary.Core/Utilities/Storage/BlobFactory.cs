using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {
  public static class BlobFactory {

    /// <summary>
    /// The internal names of the blob storage containers
    /// </summary>
    public enum Container {
      Resources,
      UserFolder,
      ProductionPreviews,
      MediaFiles
    }


    public static IBlob GetBlobStorage(Guid id, Container container) {
      var storageType = WebConfigurationManager.AppSettings["environment:StorageType"].ToLowerInvariant();
      switch (storageType) {
        case "file":
          return new FileBlob(container.ToString(), id);
        case "azure":
          return new Blob(container.ToString(), id);
        default:
          throw new ConfigurationErrorsException(storageType);
      }
    }

  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core {
  public abstract class MetadataAttribute : Attribute {
    /// <summary>
    /// Method for processing custom attribute data.
    /// </summary>
    /// <param name="modelMetaData">A ModelMetaData instance.</param>
    public abstract void Process(ModelMetadata modelMetaData);
  }
}

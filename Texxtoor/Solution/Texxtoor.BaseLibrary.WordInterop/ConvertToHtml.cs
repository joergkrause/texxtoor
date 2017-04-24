using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenXml.PowerTools;
using System.Xml.Linq;
using System.IO;

namespace Texxtoor.BaseLibrary.WordInterop {
  public class ConvertToHtml {

    # region ----==== Convert Whole Document Into Html ====----

    public void CreateHtmlFromDocument(string fileName) {
      ResourcesPackageName = "html";
      OutputPath = "";
      HtmlOutputName = System.IO.Path.GetFileNameWithoutExtension(fileName) + ".html";
      var d = OpenXmlDocument.FromFile(fileName, FileAccess.Read);
      try {
        XDocument styles = d.GetStylesDocument();
        ProcessRecord(d);
      } finally {
        d.Close();
      }
    }
    public string ResourcesPackageName {
      get;
      set;
    }

    public string HtmlOutputName {
      get;
      set;
    }

    public string OutputPath {
      get;
      set;
    }

    /// <summary>
    /// Entry point for PowerShell cmdlets
    /// </summary>
    private void ProcessRecord(OpenXmlDocument document) {
      try {
        string resolvedXslfilePath = System.IO.Path.Combine("Resources", "html.xsl");
        string resolvedOutputPath = System.IO.Path.Combine(OutputPath, ResourcesPackageName);
        try {
          document.InnerContent.TransformToHtml(false, ResourcesPackageName, HtmlOutputName, resolvedOutputPath, resolvedXslfilePath);
          document.Close();
        } catch {
          document.Close();
        }
      } catch {
      }
    }

    # endregion ----==== Convert Whole Document Into Html ====----

  }
}

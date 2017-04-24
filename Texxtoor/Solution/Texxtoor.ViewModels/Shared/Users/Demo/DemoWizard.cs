using System;
using System.Collections.Generic;
using System.Web.Helpers;

namespace Texxtoor.ViewModels.Users.Demo {

  [Serializable]
  public class DemoWizard {

    public string Id { get; set; }

    public string Image { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string DemoUrl { get; set; }

    public Demo DemoData { get; set; }

    public string Language { get; set; }
  }

  public class Demo {

    public Demo() {
      Pages = new List<Page>();
    }

    public string UserName { get; set; }
    public string Password { get; set; }

    public IList<Page> Pages { get; set; }

  }

  public class Page {

    public Page() {
      Steps = new List<string>();
    }

    public IList<string> Steps { get; set; }

    /// <summary>
    /// Create a JSON array out of the data
    /// </summary>
    public string StepArray {
      get { return String.Concat("[", String.Join(",", Steps), "]"); }
    }


    public string Url { get; set; }
  }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions {
  public class BtBoxOption : Bt {

    protected BtBoxOption(string css, bool isOption)
      : base("box-" + css) {
      IsOption = isOption;
    }

    public bool IsOption { get; set; }

    public static readonly BtBoxOption Collapse = new BtBoxOption("collapse", true);
    public static readonly BtBoxOption Remove = new BtBoxOption("remove", true);

    public static readonly BtBoxOption IsCollapsed = new BtBoxOption("collapsed", false);
  }
}
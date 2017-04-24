using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions {
  public class BtText : Bt {

    protected BtText(string css)
      : base(css) {
    }

    public static readonly BtText Muted = new BtText("text-muted");
    public static readonly BtText Primary = new BtText("text-primary");
    public static readonly BtText Warning = new BtText("text-warning");
    public static readonly BtText Danger = new BtText("text-danger");
    public static readonly BtText Success = new BtText("text-success");
    public static readonly BtText Info = new BtText("text-info");
    public static readonly BtText Error = new BtText("text-error");

    // Alignment
    public static readonly BtText Left = new BtText("left");
    public static readonly BtText Right = new BtText("right");
    public static readonly BtText Center = new BtText("center");

  }
}
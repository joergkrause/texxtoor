using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions {
  public class HtmlHeading {

    public static readonly HtmlHeading H1 = new HtmlHeading("h1");
    public static readonly HtmlHeading H2 = new HtmlHeading("h2");
    public static readonly HtmlHeading H3 = new HtmlHeading("h3");
    public static readonly HtmlHeading H4 = new HtmlHeading("h4");
    public static readonly HtmlHeading H5 = new HtmlHeading("h5");
    public static readonly HtmlHeading Heading = new HtmlHeading("heading");

    private HtmlHeading(string style) {
      Value = style;
    }

    public string Value { get; private set; }

    public static HtmlHeading operator &(HtmlHeading s1, HtmlHeading s2) {
      return new HtmlHeading(s1.Value + " " + s2.Value);
    }

    public static implicit operator string(HtmlHeading bt) {
      return bt.Value;
    }

    public override string ToString() {
      return Value;
    }


  }
}
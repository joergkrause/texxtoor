using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions {

  public class Bt {

    protected Bt(string css) {
      Value = css;
    }

    public string Value { get; protected set; }

    protected static string CreateAttributes(IDictionary<string, string> attr) {
      return String.Join(" ", attr.Select(a => String.Format(@"{0}=""{1}""", a.Key, a.Value)).ToArray());
    }

    public static Bt operator &(Bt s1, Bt s2) {
      return new Bt(s1.Value + " " + s2.Value);
    }

    public static Bt operator &(string s1, Bt s2) {
      return new Bt(s1 + " " + s2.Value);
    }

    public static Bt operator &(Bt s1, string s2) {
      return new Bt(s1.Value + " " + s2);
    }

    public static implicit operator string(Bt bt) {
      return bt.Value;
    }

    public override string ToString() {
      return Value;
    }
  }

  public class BtStyle : Bt {

    protected BtStyle(string css) : base(css) {      
    }

    public static readonly BtStyle Well = new BtStyle("well");
    public static readonly BtStyle WellSmall = new BtStyle("well-sm");
    public static readonly BtStyle WellAndSmall = new BtStyle("well well-sm");
    public static readonly BtStyle WellLarge = new BtStyle("well-lg");
    public static readonly BtStyle WellAndLarge = new BtStyle("well well-lg");

    public static readonly BtStyle Alert = new BtStyle("alert");
    public static readonly BtStyle AlertError = new BtStyle("alert-danger");
    public static readonly BtStyle AlertAndError = new BtStyle("alert alert-danger");
    public static readonly BtStyle AlertWarning = new BtStyle("alert-warning");
    public static readonly BtStyle AlertAndWarning = new BtStyle("alert alert-warning");
    public static readonly BtStyle AlertInfo = new BtStyle("alert-info");
    public static readonly BtStyle AlertAndInfo = new BtStyle("alert alert-info");
    public static readonly BtStyle AlertSuccess = new BtStyle("alert-success");
    public static readonly BtStyle AlertAndSuccess = new BtStyle("alert alert-success");

    public static readonly BtStyle Badge = new BtStyle("badge");

    public static readonly BtStyle Button = new BtStyle("btn");
    public static readonly BtStyle ButtonDefault = new BtStyle("btn-default");
    public static readonly BtStyle ButtonPrimary = new BtStyle("btn-primary");
    public static readonly BtStyle ButtonSuccess = new BtStyle("btn-success");
    public static readonly BtStyle ButtonInfo = new BtStyle("btn-info");
    public static readonly BtStyle ButtonWarning = new BtStyle("btn-warning");
    public static readonly BtStyle ButtonDanger = new BtStyle("btn-danger");
    public static readonly BtStyle ButtonAndDefault = new BtStyle("btn btn-default");
    public static readonly BtStyle ButtonAndPrimary = new BtStyle("btn btn-primary");
    public static readonly BtStyle ButtonAndSuccess = new BtStyle("btn btn-success");
    public static readonly BtStyle ButtonAndInfo = new BtStyle("btn btn-info");
    public static readonly BtStyle ButtonAndWarning = new BtStyle("btn btn-warning");
    public static readonly BtStyle ButtonAndDanger = new BtStyle("btn btn-danger");

    public static readonly BtStyle Label = new BtStyle("label");
    public static readonly BtStyle LabelDefault = new BtStyle("label-default");
    public static readonly BtStyle LabelPrimary = new BtStyle("label-primary");
    public static readonly BtStyle LabelSuccess = new BtStyle("label-success");
    public static readonly BtStyle LabelInfo = new BtStyle("label-info");
    public static readonly BtStyle LabelWarning = new BtStyle("label-warning");
    public static readonly BtStyle LabelDanger = new BtStyle("label-danger");

    public static readonly BtStyle Hidden = new BtStyle("hidden-to-show");

  }
}
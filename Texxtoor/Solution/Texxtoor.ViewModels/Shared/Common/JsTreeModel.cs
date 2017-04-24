namespace Texxtoor.ViewModels.Common {
  // ReSharper disable InconsistentNaming
  public class JsTreeModel {
    public string data;
    public JsTreeAttribute attr;
    public JsTreeModel[] children;
  }

  public class JsTreeAttribute {
    public string id;
    public string rel;
    public string dataitem;
    public string datatext;
  }
  // ReSharper restore InconsistentNaming
}

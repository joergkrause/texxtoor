namespace Texxtoor.Editor.ViewModels {
  // ReSharper disable InconsistentNaming
  public class JsTreeModel {
    public string data;
    public JsTreeAttribute attr;
    public JsTreeModel[] children;
  }

  public class JsTreeAttribute {
    public string id;
    public string rel;
  }
  // ReSharper restore InconsistentNaming
}

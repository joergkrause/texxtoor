using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.ViewModels.Reader {

  public class BookContentFragment {

    public string Content { get; set; }

    public IList<BookContentFragment> Children { get; set; }

    public bool HasChildren { 
      get {
        return !(Children == null || !Children.Any());
      }
    }

    public int Size { get { return ((Content == null) ? 0 : Content.Length); } }

  }

  public class BookResourceFragment {

    public string ItemRef { get; set; }
    public byte[] Content { get; set; }
    public int Size { get { return ((Content == null) ? 0 : Content.Length); } }
  }

  /// <summary>
  /// A by level limited collection of actual data that can be used in all reader apps
  /// </summary>
  public class BookContent {

    public BookContent(Work work) : this(work, 2) {
    }

    public BookContent(Work work, int level) {
      // children form a linear list
      Content = new List<BookContentFragment>();
      GetFragmentsFromCollection(work);
    }

    private string GetFragmentsFromCollection(Work coll) {
      Func<List<FrozenFragment>, string[]> func = null;
      func = e => e
          .OrderBy(s => s.OrderNr)
          .Select(s => String.Concat(string.Format("{0}{1}", CreateDataFragment(s), (s.HasChildren() ? String.Join(Environment.NewLine, func(s.Children)) : String.Empty))))
          .ToArray();
      // levelizer
      if (coll.Fragments.Any()) {
        return "";
      } return null;
    }

    private string CreateDataFragment(FrozenFragment fragment) {
      // identify fragment
      // get attribute
      string content;
      switch (fragment.TypeOfFragment) {
        case FragmentType.Html:
          content = System.Text.Encoding.UTF8.GetString(fragment.Content);
          break;
        default:
          // TODO: Implement this
          throw new NotImplementedException();
      }
      return content;
    }

    private IList<BookContentFragment> Content { get; set; }

  }
}

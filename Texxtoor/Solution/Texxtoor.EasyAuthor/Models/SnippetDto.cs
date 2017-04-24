using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.EasyAuthor.Models {
  public class SnippetDto {

    public SnippetDto() {
      _levelNumbering = new Dictionary<int, int>();
      for (int i = 1; i <= 8; i++) {
        _levelNumbering.Add(i, 1);
      }      
    }

    public SnippetDto(Section s) : this() {
      SectionId = s.Id;
      Title = Encoding.UTF8.GetString(s.Content);
      Level = s.Level;
      HasChildren = s.HasChildren();
      HasContent = s.Children.OfType<Snippet>().Any();
      OrderNumber = GetParentNumbering(s);
      IsChapter = s.Parent is Opus;
    }

    private readonly IDictionary<int, int> _levelNumbering = null;

    private string GetParentNumbering(Section s) {
      var current = (1).ToString();
      if (s.Parent is Opus) {
        current = (s.Parent.Children.IndexOf(s) + 1).ToString();
      }
      if (s.Parent is Section) {
        current = s.Parent.Children.IndexOf(s).ToString();
        current = GetParentNumbering(s.Parent as Section) + "." + current;
      }
      return current;
    }

    public int SectionId { get; set; }

    public bool IsChapter { get; set; }

    public string OrderNumber { get; set; }

    public string Title { get; set; }

    public int Level { get; set; }

    public bool HasChildren { get; set; }

    public bool HasContent { get; set; }


  }
}

using System;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public class EditorServiceWrapperAttribute : Attribute {
    public EditorServiceWrapperAttribute(Type t) {
      Type = t;
    }

    public Type Type { get; set; }

    public JsonBehavior GetJson(Opus doc, Snippet chapter, Snippet snippet) {
      if (snippet is Section) {
        var numberChain = doc.ShowNumberChain
                            ? snippet.GetSectionNumber(chapter.Id)
                            : snippet.GetSectionLevel(chapter.Id);
        return new SectionJsonBehavior().GetJson(doc, chapter, snippet as Section, numberChain);
      }
      if (snippet is ImageSnippet)
        return new ImageJsonBehavior().GetJson(doc, chapter, snippet);
      if (snippet is SidebarSnippet)
        return new SidebarJsonBehavior().GetJson(doc, chapter, snippet);
      if (snippet is TextSnippet)
        return new TextJsonBehavior().GetJson(doc, chapter, snippet);
      if (snippet is ListingSnippet)
        return new ListingJsonBehavior().GetJson(doc, chapter, snippet);
      if (snippet is TableSnippet)
        return new TableJsonBehavior().GetJson(doc, chapter, snippet);
      return null;
    }
  }
}

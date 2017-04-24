using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Texxtoor.ViewModels.Editor {

  public class Command {

    public Command(string name, string text, string tooltip) {
      Name = name;
      ButtonText = text;
      ButtonTooltip = tooltip;
    }

    public string Name { get; set; }
    public string ButtonText { get; set; }
    public string ButtonTooltip { get; set; }
    public string Executable { get; set; }
    public string Icon { get; set; }
  }

  public class ToolSetModel {

    private List<Command> inlineElements = new List<Command>();
    private List<Command> currentInlineElements = new List<Command>();

    private List<Command> listElements = new List<Command>();
    private List<Command> currentListElements = new List<Command>();

    private List<Command> snippetElements = new List<Command>();
    private List<Command> currentSnippetElements = new List<Command>();

    public bool UserIsTeamLead { get; set; }

    public ToolSetModel() {
      inlineElements.AddRange(new Command[] {
        // command   text   tooltip
        new Command("a", "Hyperlink", "Several server supported lists"),
        new Command("address", "Address", "Real World Address"),
        new Command("bold", "Bold", "non-semantic element"),
        new Command("br", "Break", "Forced line break"),
        new Command("code", "Code", "Inline code"),
        new Command("command", "Command", "Inline menu item"),
        new Command("details", "Details", "Explains a word inline"),
        new Command("em", "Emphasize", "Emphasis (semantic)"),
        new Command("italic", "Idiom", "Semantic, can be auto-linked to Wikipedia and is server supported"),
        new Command("kbd", "Keyboard", "Keyboard strokes"),
        new Command("li", "List", "Element in a list"),
        new Command("meter", "Meter", "Value on a know scale"),
        new Command("ol", "List", "Ordered list"),
        new Command("pre", "Pre-formatted text", ""),
        new Command("q", "Quote", "direct speech in text (quote)"),
        new Command("samp", "Sample", "sample that the surrounding text refers to"),
        new Command("strong", "Strong", "semantically strong emphasis	"),
        new Command("subscript", "Sub", "sub text"),
        new Command("superscript", "Super", "super text"),
        new Command("time", "Date", "date / time"),
        new Command("ul", "List", "Unsorted list"),
      });

      listElements.AddRange(new Command[] {
        // command   text   tooltip
        new Command("abbr", "Abbreviation", "Server supported list"),
        new Command("cite", "Cite", "Server supported list"),
        new Command("var", "Variable", "Variable name from server supported list")
      });

      snippetElements.AddRange(new Command[] {
        new Command("section", "Section on same level", ""),
        new Command("subsection", "Sub Section", ""),
        new Command("aside", "Side Step", "Side Step related to the given fragment"),
        new Command("p", "Paragraph", "")
       });
    }

    public List<Command> GetAllInlineElements() {
      return inlineElements;
    }

    public List<Command> GetAllSnippetElements() {
      return snippetElements;
    }

    public List<Command> GetAllListElements() {
      return listElements;
    }
      
    [NotMapped]
    public List<Command> CurrentInlineElements { get { return currentInlineElements; } }

    [NotMapped]
    public List<Command> CurrentSnippetElements { get { return currentSnippetElements; } }

    [NotMapped]
    public List<Command> CurrentListElements { get { return currentListElements; } }
  }


}
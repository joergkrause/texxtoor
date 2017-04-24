using Texxtoor.BaseLibrary.Core.Logging;

namespace Texxtoor.ViewModels.Users {
  /// <summary>
  /// Used to send JSON formatted data to search forms. The JQuery tokenizer requires lower case properties.
  /// </summary>
  public class EditUser : IViewModel {
    /// <summary>
    /// ID
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// User name (internal logon name)
    /// </summary>
    public string name { get; set; }

  }
}
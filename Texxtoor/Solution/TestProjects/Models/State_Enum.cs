using System;

namespace LinqDemo.Models {

  [Flags]
  public enum State {

    Unknown = 0,
    /// <summary>
    /// Author is working on it
    /// </summary>
    InProgress = 1,
    /// <summary>
    /// Other contributors shall work on it
    /// </summary>
    WatingForInput = 2,
    /// <summary>
    /// The up level (Previous Version) element shall provide content
    /// </summary>
    UseUplevelContent = 4,
    /// <summary>
    /// Author is done and no further editing is allowed
    /// </summary>
    ClosedbyAuthor = 8,
    /// <summary>
    /// An automated process is working
    /// </summary>
    AutomatedWaiting = 64,
    /// <summary>
    /// The automated process is done (Automated + Closed)
    /// </summary>
    AutomatedDone = 128,
    
  }
}

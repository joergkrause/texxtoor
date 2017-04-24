using System;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Helper {

  /// <summary>
  /// While creating a document the images are treated in different ways. Some builders need them in a temp folder, others expect a stream. This handler
  /// is provided by the caller and helps rendering resources.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  /// <returns>The path or Id that references the processed resource.</returns>
  public delegate string CreateImageHandler(object sender, CreateImageArguments e);

  /// <summary>
  /// Some builder targets needs to scale and apply properties while creating the target. This handler takes care of the application.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  public delegate void ScaleImageHandler(object sender, ScaleImageEventArgs e);

  /// <summary>
  /// The arguments to control the handler
  /// </summary>
  public class CreateImageArguments : ScaleImageEventArgs {

    /// <summary>
    /// The filename of the resource in the source environment.
    /// </summary> 
    public string FileName { get; set; }
    /// <summary>
    /// A temp path to store the resource if required.
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// The actual content being processed
    /// </summary>
    public byte[] Content { get; set; }
  }

  public class ScaleImageEventArgs : EventArgs {
    /// <summary>
    /// The image properties that might be applied while processing. The properties are written by the editor.
    /// </summary>
    public ImageProperties Properties { get; set; }
    /// <summary>
    /// The complete source snippet.
    /// </summary>
    public Snippet SourceSnippet { get; set; }
    /// <summary>
    /// The optional target fragment. For previews this may remain <c>null</c>.
    /// </summary>
    public FrozenFragment TargetFragment { get; set; }

  }

}

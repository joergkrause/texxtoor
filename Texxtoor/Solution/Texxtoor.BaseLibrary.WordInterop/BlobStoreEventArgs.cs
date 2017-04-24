using System;
using System.IO;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BaseLibrary.WordInterop {

  /// <summary>
  /// Handles the image snippet management. An image found in Word file is pulled from blob store and copied as JPEG to blob store at working location. The new GUID is 
  /// what the handler returns if successful. In case of any error it will return null.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  /// <returns></returns>
  public delegate Guid? BlobStoreEventHandler(object sender, BlobStoreEventArgs e);

  public class BlobStoreEventArgs : EventArgs {

    public int ProjectId { get; set; }
    public string Name { get; set; }
    public string TypeName { get; set; }
    public ResourceFile Item { get; set; }
    public Stream RawData { get; set; }
  }
}

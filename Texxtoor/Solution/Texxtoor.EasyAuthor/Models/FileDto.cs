using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.EasyAuthor.Models {
  public class FileDto {

    public int Id { get; set; }
    public string MimeType { get; set; }

    public string Name { get; set; }

    public string Size { get; set; }

    public string Date { get; set; }



    public string Label { get; set; }

    public IEnumerable<string> Labels { get; set; }

    public string Volume { get; set; }

    public bool IsImage { get; set; }

    public bool IsPublished { get; set; }
  }

  public class FolderDto {


    public int Id { get; set; }

    public string Name { get; set; }

    public IEnumerable<FileDto> Files { get; set; }

    public IEnumerable<FolderDto> Folders { get; set; }

    public string Volume { get; set; }

  }

}
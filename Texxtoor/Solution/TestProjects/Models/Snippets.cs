using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace LinqDemo.Models {

  [Table("Elements")]
  public class TextSnippet : Snippet {

  }

  [Table("Elements")]
  public class SidebarSnippet : TextSnippet {


  }

}

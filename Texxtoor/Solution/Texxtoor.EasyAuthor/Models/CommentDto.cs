using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Texxtoor.EasyAuthor.Models {
  public class CommentDto {

    public string Subject { get; set; }

    public string Text { get; set; }

    public string UserName { get; set; }


  }
}
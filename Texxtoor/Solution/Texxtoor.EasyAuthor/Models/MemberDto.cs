using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.EasyAuthor.Models {
  public class MemberDto {

    public int MemberId { get; set; }

    public IEnumerable<string> Roles { get; set; }

    public string FullName { get; set; }

    public string ThumbnailPath { get; set; }

  }
}

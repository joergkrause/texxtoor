using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  /// <summary>
  /// Save private and public comments for each fragment. Might build an hierarchy by itself to have forum like threads.
  /// </summary>
  [Table("Comments", Schema = "Reader")]
  public class Comment : Discussion<Comment> {

    /// <summary>
    /// Work this discussion thread is associated to.
    /// </summary>
    public Work Work { get; set; }

    /// <summary>
    /// Reference where the comment appears in the work using EPUB CFI syntax.
    /// </summary>
    public string CfiRef { get; set; }

    //[NotMapped]
    //public bool ToAuthor {
    //  get {
    //    return CommentType == CommentsType.ToAuthor;
    //  }
    //  set {
    //    if (value) {
    //      CommentType = CommentsType.ToAuthor;
    //    }
    //  }
    //}

    //[NotMapped]
    //public override bool Private {
    //  get {
    //    return CommentType == CommentsType.Private;
    //  }
    //  set {
    //    if (value) {
    //      CommentType = CommentsType.Private;
    //    }
    //  }
    //}

    //[NotMapped]
    //public override bool GroupOnly {
    //  get {
    //    return CommentType == CommentsType.GroupOnly;
    //  }
    //  set {
    //    if (value) {
    //      CommentType = CommentsType.GroupOnly;
    //    }
    //  }
    //}

    //public CommentsType CommentType { get; set; }

  }


}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Models.Users {

  [Table("UserRating", Schema = "Common")]
  public class UserRating : EntityBase {

    public UserRating() {
    }

    //public UserRating(User currentUser) {
    //  Friendlyness = 5;
    //  Communication = 5;
    //  Punctuality = 5;
    //  Quality = 5;
    //  Reliability = 5;
    //  RelatedProject = null;
    //  RatingUserProfile = currentUser.Profile;
    //}

    public decimal GetAverage() {
      return Math.Round(this.GetType().GetProperties().Where(p => p.PropertyType.Name == "Int32").Average(p => Convert.ToDecimal(p.GetValue(this, null) ?? 0)), 1);
    }

    [Range(0, 10)]
    public int Reliability { get; set; }
    [Range(0, 10)]
    public int Communication { get; set; }
    [Range(0, 10)]
    public int Quality { get; set; }
    [Range(0, 10)]
    public int Friendlyness { get; set; }
    [Range(0, 10)]
    public int Punctuality { get; set; }

    //// The user to this rating applies
    //[Required]
    //public UserProfile RatingUserProfile { get; set; }

    // an optional comment
    [StringLength(1024)]
    [UIHint("Textarea")]
    [AdditionalMetadata("rows", 3)]
    [AdditionalMetadata("cols", 60)]
    public string Comment { get; set; }

    // Project for which this rating was invoked
    public Project RelatedProject { get; set; }

  }

}
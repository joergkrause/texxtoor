using System;
using System.Collections.Generic;
using Texxtoor.DataModels.Models;

namespace Texxtoor.ViewModels.Author {

  // an aggregated value for user rating
  public class AverageRating {
    public decimal Average { get; set; }
    public decimal Reliability { get; set; }
    public decimal Communication { get; set; }
    public decimal Quality { get; set; }
    public decimal Friendlyness { get; set; }
    public decimal Punctuality { get; set; }
  }
}

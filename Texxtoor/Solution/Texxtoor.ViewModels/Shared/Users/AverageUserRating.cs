using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.ViewModels.Users {
  public class AverageUserRating {

    public string ProjectName { get; set; }

    public double Reliability { get; set; }
    public double Communication { get; set; }
    public double Quality { get; set; }
    public double Friendlyness { get; set; }
    public double Punctuality { get; set; }

  }
}

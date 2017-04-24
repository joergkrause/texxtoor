using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Marketing;
using System;
using System.Web;

namespace Texxtoor.BusinessLayer.Attributes {

  /// <summary>
  /// The purpose of this attribute is to gather activity to determine the state of a user (gold, silver, ...)
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
  public class UserActivityAttribute : Attribute {

    public int OperationValue { get; set; }

    public string OperationType { get; set; }

    public UserActivityAttribute(string operationType, int operationValue) {
      OperationType = operationType;
      OperationValue = operationValue;
      var iid = HttpContext.Current.User.Identity;
      // only authenticated users
      if (iid.IsAuthenticated) {
        try {
          Manager<UserManager>.Instance.SaveChanges();
        } catch (System.Exception) {
        }
        try {
          Manager<UserManager>.Instance.GatherUserActivity(OperationValue, OperationType, iid.Name);
        } catch (System.Exception) {
        }
      }
    }

  }
}
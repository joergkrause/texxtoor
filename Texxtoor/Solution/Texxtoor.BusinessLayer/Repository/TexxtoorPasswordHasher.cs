using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Texxtoor.BaseLibrary.Services;

namespace Texxtoor.BaseLibrary.Repository {

  /// <summary>
  ///  The purpose of this is to make the identity implementation backwards compatible with existing passwords on the platform create with old membership.
  /// </summary>
  public class TexxtoorPasswordHasher : PasswordHasher {

    public string FormsAuthPasswordFormat { get; set; }

    public TexxtoorPasswordHasher(string format) {
      FormsAuthPasswordFormat = format;
    }

    public override string HashPassword(string password) {
      return TexxtoorMembershipService.CreateHash(password, FormsAuthPasswordFormat);
    }

    public override PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword) {
      var testHash = TexxtoorMembershipService.CreateHash(providedPassword, FormsAuthPasswordFormat);
      return hashedPassword.Equals(testHash) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
  }
}

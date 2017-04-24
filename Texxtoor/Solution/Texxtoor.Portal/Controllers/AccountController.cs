using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Shared.Users.Account;
using Texxtoor.ViewModels.Users;


namespace Texxtoor.Portal.Controllers {

  public class AccountController : ControllerExt {


    // **************************************
    // URL: /Account/LogOn
    // **************************************
    [AllowAnonymous]
    [NavigationPathFilter("Log On")]
    public ActionResult LogOn() {
      var host = Request.Url.Host.Split('.');
      if (host.Count() == 3) {
        ViewBag.Domain = host[2];
      } else {
        ViewBag.Domain = "Nothing";
      }
      return View(new LogOn());
    }

    [AllowAnonymous]
    public ActionResult LogOnMenu() {
      var model = new LogOn {
        UserName = UserName,
        Password = ""
      };
      return PartialView("_LogOnMenu", model);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> LogOn(LogOn model, string returnUrl) {
      if (ModelState.IsValid) {
        if (IsEmail(model.UserName)) {
          model.UserName = UnitOfWork<UserManager>().GetUserByEmail(model.UserName).UserName;
        }
        if (await UnitOfWork<UserManager>().ValidateUser(model.UserName.Trim(), model.Password.Trim())) {
          var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(model.UserName);
          var user = profile.User;
          user.SecurityStamp = Guid.NewGuid().ToString();
          await UnitOfWork<UserManager>().SignInAsync(user, false);
          UnitOfWork<UserManager>().LogSignIn(model.UserName, model.RememberMe);
          // broadcast new userlist to all clients
          if (Url.IsLocalUrl(returnUrl)) {
            return Redirect(returnUrl);
          }
          return Redirect(Url.Action("Index", "Home"));
        }

        ModelState.AddModelError("", ControllerResources.AccountController_LogOn_The_user_name_or_password_provided_is_incorrect);
      }
      UnitOfWork<UserManager>().RegisterFailedAttempt(model.UserName);
      // If we got so far, something failed, redisplay form
      return View(model);
    }

    public ActionResult RaiseModeToFull() {
      UnitOfWork<UserManager>().RaiseModeToFull(true, UserName);
      return RedirectToAction("Index", "Home", new { area = "" });
    }

    public ActionResult LogOff() {
      UnitOfWork<UserManager>().SignOut();
      // broadcast new userlist to all clients
      return RedirectToAction("Index", "Home", new { area = "" });
    }

    [AllowAnonymous]
    [NavigationPathFilter("Register")]
    public ActionResult Register(string name, string email) {
      ViewBag.HasError = false;
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      var r = new RegisterViewModel();
      if (name != null) {
        r.Email = email;
        r.UserName = name;
      }
      return View(r);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Register(RegisterViewModel model) {
      ViewBag.HasError = false;
      if (UnitOfWork<UserManager>().IsExistingEmail(model.Email)) {
        ModelState.AddModelError("", ControllerResources.AccountController_Register_An_account_with_this_email_already_exists_);
      }
      if (ModelState.IsValid) {
        // let's user refine this in the profile
        var user = new User {
          UserName = model.UserName,
          Email = model.Email,
          Password = TexxtoorMembershipService.CreateHash(model.Password, TexxtoorMembershipService.HashAlgorithmType),
          PhoneNumber = model.Phone,
          PasswordQuestion = model.PasswordQuestion,
          PasswordAnswer = model.PasswordAnswer,
          PasswordFormat = MembershipPasswordFormat.Hashed
        };
        var result = await UnitOfWork<UserManager>().Usermanager.CreateAsync(user, model.Password);
        if (result.Succeeded) {
          user.SecurityStamp = Guid.NewGuid().ToString();
          await UnitOfWork<UserManager>().SignInAsync(user, false);
          UnitOfWork<UserManager>().LogSignIn(model.UserName, true);
          return RedirectToAction("WelcomeNewUser", model);
        }
        ViewBag.Status = TexxtoorMembershipService.ErrorCodesToString(result.Errors);
      }
      // If we got this far, something failed, redisplay form
      ViewBag.HasError = true;
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      return View(model);
    }


    public ActionResult WelcomeNewUser(RegisterViewModel model) {
      return View(model);
    }

    [AllowAnonymous]
    public ActionResult ResendActivationMail() {
      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult ResendActivationMail(ResendMail model) {
      if (ModelState.IsValid) {
        var result = UnitOfWork<UserManager>().ResendActivationMail(model);
        if (!result) {
          ViewBag.SendError =
            ControllerResources.AccountController_ResendActivationMail_The_name_provided_has_not_been_found;
        }
        return View(model);
      }
      return View();
    }

    [Authorize]
    public ActionResult ChangePassword() {
      ViewBag.HasError = false;
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      return View();
    }

    [Authorize]
    [HttpPost]
    public ActionResult ChangePassword(ChangePassword model) {
      ViewBag.HasError = false;
      if (ModelState.IsValid) {
        if (UnitOfWork<UserManager>().ChangePassword(UnitOfWork<UserManager>().GetCurrentUserName(), model.OldPassword, model.NewPassword)) {
          return View("ChangePasswordSuccess");
        }
        ViewBag.HasError = true;
        ModelState.AddModelError("", ControllerResources.AccountController_ChangePassword_The_current_password_is_incorrect_or_the_new_password_is_invalid_);
      }

      // If we got this far, something failed, redisplay form
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      return View(model);
    }

    [AllowAnonymous]
    public ActionResult RetrievePassword() {
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      ViewBag.HasError = false;
      return View(new RetrievePassword());
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult RetrievePassword(RetrievePassword model) {
      ViewBag.HasError = false;
      if (ModelState.IsValid) {
        if (UnitOfWork<UserManager>().RetrievePassword(model.UserName, model.PasswordAnswer)) {
          ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
          return RedirectToAction("RetrievePasswordSuccess");
        }
        ViewBag.HasError = true;
        ModelState.AddModelError("", ControllerResources.AccountController_RetrievePassword_The_current_password_is_incorrect_or_the_new_password_is_invalid_);
      }

      // If we got this far, something failed, redisplay form
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      return View(model);
    }

    public ActionResult RetrievePasswordSuccess() {
      ViewBag.HasError = false;
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      return View(new SetPassword());
    }

    [HttpPost]
    public ActionResult RetrievePasswordSuccess(SetPassword model) {
      ViewBag.HasError = false;
      if (ModelState.IsValid) {
        if (UnitOfWork<UserManager>().ChangePassword(model.UserName, model.OldPassword, model.NewPassword)) {
          return View("ChangePasswordSuccess");
        }
        ViewBag.HasError = true;
        ModelState.AddModelError("", ControllerResources.AccountController_ChangePassword_The_current_password_is_incorrect_or_the_new_password_is_invalid_);
      }

      // If we got this far, something failed, redisplay form
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      return View(model);
    }



    // **************************************
    // URL: /Account/ChangePasswordSuccess
    // **************************************

    [Authorize]
    [HttpGet]
    [NavigationPathFilter("Profile")]
    public ActionResult UserProfile() {
      var model = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      if (model == null) {
        var user = UnitOfWork<UserManager>().GetCurrentUser(UserName);
        model = new UserProfile { Addresses = new List<AddressBook>(), User = user, PayPalUserId = user.Email };
      }
      ViewBag.HasError = false;
      ViewBag.Genders = UnitOfWork<UserProfileManager>().GetGenderList(model);
      return View(model);
    }

    [ValidateInput(false)]
    [HttpPost]
    public JsonResult UserProfile(UserProfile profile, int? gender) {
      var result = UnitOfWork<UserProfileManager>().SaveProfile(profile, gender, UserName);
      if (result) {
        ViewBag.HasError = false;
        return Json(new { msg = ControllerResources.AccountController_Userprofile_Saved });
      }
      ViewBag.HasError = true;
      return Json(new { msg = ControllerResources.AccountController_Userprofile_SavedError });
    }

    [HttpPost]
    public JsonResult AddTimeslot(int id, Texxtoor.DataModels.Models.Users.UserProfile.AvailabilityFrame frame) {
      var profile = UserProfileManager.Instance.GetProfile(id);
      if (frame.StartAvailability < DateTime.Today) {
        return Json(new { msg = "Not allowed to set dates in the past", err = true });
      }
      // exclusive only, because we can assume END > START, START AND END must be before START or after END existing
      var slots = !profile.FutureAvailabilities.All(f => frame.EndAvailability < f.StartAvailability || frame.StartAvailability > f.EndAvailability);
      if (!slots) {
        profile.Availabilities.Add(frame);
        UserProfileManager.Instance.SaveChanges();
        return Json(new { msg = "OK", err = false });
      }
      else {
        return Json(new { msg = "The give space is already occupied, slot not saved", err = true });
      }      
    }

    [HttpPost]
    public JsonResult RemoveTimeslot(int id, Texxtoor.DataModels.Models.Users.UserProfile.AvailabilityFrame frame) {
      var profile = UserProfileManager.Instance.GetProfile(id);
      var slot = profile.Availabilities.SingleOrDefault(a => a.StartAvailability == frame.StartAvailability && a.EndAvailability == frame.EndAvailability);
      if (slot != null) {
        profile.Availabilities.Remove(slot);
        UserProfileManager.Instance.SaveChanges();
        return Json(new { msg = "OK", err = false });
      }
      else {
        return Json(new {msg = "Slot not found. Please check date values.", err = true});
      }
    }

    [HttpGet]
    public JsonResult GetTimeslots(int id) {
      var profile = UserProfileManager.Instance.GetProfile(id);
      var json = profile.Availabilities
        .Where(a => a.StartAvailability > DateTime.Now)
        .GroupBy(a => new { a.GetKindLocalizedForGrouping, a.Name })
        .Select(a => new {
          name = a.Key.GetKindLocalizedForGrouping,
          desc = a.Key.Name,
          values = a.Select(v => new {
            from = v.StartAvailability,
            to = v.EndAvailability,
            label = v.Name,
            customClass = v.Kind // TODO: deliver class names
          }).ToArray()
        });
      return Json(json, JsonRequestBehavior.AllowGet);
    }

    # region Linking Accounts

    public ActionResult LinkConfirmation()
    {
      var user = UserManager.Instance.GetUserByName(UserName);
      if (user != null)
      {
        var model = new ConfirmAssociation
        {
          Email = user.Email,
          LinkCode = user.LinkSalt
        };
        return View("External/LinkConfirmation", model);
      }
      return View("LogOn");
    }

    public ActionResult ConfirmAssociation(ConfirmAssociation model)
    {
      var um = UserManager.Instance;
      var user = um.GetUserByName(UserName);
      var linkedUser = um.GetUserByEmail(model.Email);
      if (user != null && linkedUser != null)
      {
        linkedUser.LeadingAccountId = user.Id;
        um.SaveChanges();
      }
      return View("External/ConfirmAssociation");
    }

    # endregion

    # region Matrix Handling - This are the cross links between producer and consumer (match matrix)

    public ActionResult ConsumerMatrix(int id) {
      var p = UnitOfWork<UserProfileManager>().GetProfile(id, e => e.ConsumerMatrix);
      return PartialView("Profile/Fragments/_ConsumerMatrix", p.ConsumerMatrix.Where(c => c.Temporary == false));
    }

    public ActionResult ContributorMatrix(int id) {
      var p = UnitOfWork<UserProfileManager>().GetProfile(id, e => e.LanguageMatrix, e => e.ContributorMatrix);
      ViewBag.LanguageMatrixList = p.LanguageMatrixList;
      return PartialView("Profile/Fragments/_ContributorMatrix", p.ContributorMatrix);
    }

    public ActionResult LanguageMatrix(int id) {
      var p = UnitOfWork<UserProfileManager>().GetProfile(id, e => e.LanguageMatrix);
      return PartialView("Profile/Fragments/_LanguageMatrix", p.LanguageMatrix);
    }

    [HttpPost]
    public JsonResult ConsumerMatrixAdd(int id) {
      UnitOfWork<UserProfileManager>().AddConsumerMatrixToProfile(id);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Added });
    }

    [HttpPost]
    public ActionResult ContributorMatrixAdd(int id) {
      UnitOfWork<UserProfileManager>().AddContributorMatrixToProfile(id);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Added });
    }

    [HttpPost]
    public ActionResult LanguageMatrixAdd(int id) {
      UnitOfWork<UserProfileManager>().AddLanguageMatrixToProfile(id);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Added });
    }

    [HttpPost]
    public JsonResult ConsumerMatrixEdit(int id, int matrix, string keyword, int targets, int stages) {
      // id == profileId
      UnitOfWork<UserProfileManager>().EditConsumerMatrixToProfile(id, matrix, keyword, targets, stages);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Changed });
    }

    [HttpPost]
    public ActionResult ContributorMatrixEdit(int id, int matrix, string language, ContributorRole role, string information) {
      UnitOfWork<UserProfileManager>().EditContributorMatrixToProfile(id, matrix, language, role, information);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Changed });
    }

    [HttpPost]
    public ActionResult LanguageMatrixEdit(int id, int matrix, string language, int levels) {
      UnitOfWork<UserProfileManager>().EditLanguageMatrixToProfile(id, matrix, language, levels);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Changed });
    }

    [HttpPost]
    public JsonResult ConsumerMatrixRemove(int id, int matrixId) {
      UnitOfWork<UserProfileManager>().RemoveConsumerMatrixToProfile(id, matrixId);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Removed });
    }

    [HttpPost]
    public ActionResult ContributorMatrixRemove(int id, int matrixId) {
      UnitOfWork<UserProfileManager>().RemoveContributorMatrixToProfile(matrixId);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Removed });
    }

    [HttpPost]
    public ActionResult LanguageMatrixRemove(int id, int matrixId) {
      UnitOfWork<UserProfileManager>().RemoveLanguageMatrixToProfile(matrixId);
      return Json(new { msg = ControllerResources.AccountController_Matrix_Removed });
    }

    # endregion

    [HttpGet]
    public ActionResult Contact(string target = null) {
      var model = new ContactModel();
      if (!String.IsNullOrEmpty(UserName)) {
        var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);

        if (profile != null) {
          model.Name = String.Format("{0} {1}", profile.FirstName, profile.LastName);
          model.EMail = profile.User.Email;
        } else {
          model.Name = UserName;
        }
      }
      ViewBag.Received = false;
      ViewBag.Target = target;
      return View(model);
    }

    [HttpPost]
    public ActionResult Contact(ContactModel model) {
      if (ModelState.IsValid) {
        UnitOfWork<UserProfileManager>().AddContactFormRequest(model, UserName);
        ViewBag.Received = true;
      } else {
        ViewBag.Received = false;
      }
      return View(model);
    }

    [Authorize]
    [NavigationPathFilter("Archive")]
    public ActionResult Archive() {
      return View();
    }

    [Authorize]
    [NavigationPathFilter("RecycleBin")]
    public ActionResult RecycleBin() {
      return View();
    }

    [Authorize]
    [NavigationPathFilter("Archive")]
    public ActionResult Private() {
      return View("Archive");
    }

    public ActionResult ListUserFiles(PaginationFormModel p) {
      var uf = OrderManager.Instance.GetUserFiles(UserName);
      ViewBag.RecycleBin = false;
      return PartialView("Archive/_ListUserFiles", uf.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ListRecycleBin(PaginationFormModel p) {
      var uf = OrderManager.Instance.GetRecycleBin(UserName);
      ViewBag.RecycleBin = true;
      return PartialView("Archive/_ListUserFiles", uf.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    [HttpPost]
    public JsonResult DeleteUserFile(int id) {
      OrderManager.Instance.DeleteUserFile(id, UserName);
      return Json(new { msg = ControllerResources.AccountController_DeleteUserFile_File_moved_to_recycle_bin_ });
    }

    [HttpPost]
    public JsonResult DeleteUserFilePermanently(int id) {
      OrderManager.Instance.DeleteUserFilePermanently(id, UserName);
      return Json(new { msg = ControllerResources.AccountController_DeleteUserFilePermanently_File_removed_from_recycle_bin_ });
    }

    [HttpPost]
    public JsonResult RecoverUserFile(int id) {
      OrderManager.Instance.RecoverUserFile(id, UserName);
      return Json(new { msg = ControllerResources.AccountController_RecoverUserFile_File_is_active_now_ });
    }

    public ActionResult DownloadFile(int id) {
      var uf = OrderManager.Instance.GetUserFile(id, UserName);
      BlobFactory.Container container;
      IBlob blob;
      if (Enum.TryParse(uf.Folder, out container)) {
        blob = BlobFactory.GetBlobStorage(uf.ResourceId, container);
      } else {
        blob = BlobFactory.GetBlobStorage(uf.ResourceId, BlobFactory.Container.UserFolder);
      }
      var mt = uf.MimeType ?? "application/unknown";
      if (blob.Content != null) {
        return File(blob.Content, mt, uf.Name);
      }
      return RedirectToAction("UserFileEmpty");
    }

    public ActionResult UserFileEmpty() {
      return View();
    }

    public ActionResult ConfirmMail() {
      var registerId = Request.RequestContext.RouteData.Values["path"].ToString();
      Guid test;
      if (!Guid.TryParse(registerId, out test)) {
        return View();
      }
      var result = UnitOfWork<UserManager>().ConfirmMail(test.ToString());
      // until simple mode is not ready we force full mode
      if (result) {
        var user = UnitOfWork<UserManager>().GetUserByName(UserName);
        return View(user);
      }
      UnitOfWork<UserManager>().SignOut();
      return RedirectToAction("ResendActivationMail");
    }

    [AllowAnonymous]
    public ActionResult QuickFormInfoText() {
      ViewBag.QuickConfirm = false;
      return View(new QuickForm());
    }

    [AllowAnonymous]
    public ActionResult QuickForm() {
      ViewBag.QuickConfirm = false;
      return PartialView("_QuickForm", new RegisterViewModel());
    }


    [AllowAnonymous]
    [HttpPost]
    [ValidateInput(false)]
    public JsonResult QuickForm(RegisterViewModel model) {
      if (ModelState.IsValid) {
        RedirectToAction("Register", model);
        //await SendQuickForm(form);
        ViewBag.QuickConfirm = true;
        return Json(new { msg = "OK", err = false });
      }
      ViewBag.QuickConfirm = false;
      return Json(new { msg = ControllerResources.AccountController_QuickForm_Fehler_beim_Ausfüllen_der_Form_, err = true });
    }

    public JsonResult CheckUser(string name) {
      var user = UnitOfWork<UserManager>().GetUserByName(name);
      if (user == null) {
        return Json(new {
          name = name,
          question = ""
        }, JsonRequestBehavior.AllowGet);
      } else {
        return Json(new {
          name = user.UserName,
          question = user.PasswordQuestion
        }, JsonRequestBehavior.AllowGet);
      }
    }

    # region --== Addresses ==--

    public ActionResult ListAddresses(PaginationFormModel p) {
      var user = UnitOfWork<UserManager>().GetUserByName(UserName);
      var adrs = UnitOfWork<UserProfileManager>().GetAllAddressedForUser(user.Id);
      return PartialView("Profile/Address/_ListAddress", adrs.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult AddAddress() {
      var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      var name = String.Format("{0} {1}", profile.FirstName, profile.LastName);
      if (name == " ") {
        name = UserName;
      }
      var adr = new AddressBook {
        Name = name
      };
      return PartialView("Profile/Address/_AddAddress", adr);
    }

    [HttpPost]
    public JsonResult AddAddress(AddressBook adr, int countryId, string region) {
      UnitOfWork<UserProfileManager>().SetOrAddAddressForUser(adr, countryId, region, UserName);
      return Json(new { msg = "Saved" });
    }

    public ActionResult EditAddress(int id) {
      var adr = UnitOfWork<UserProfileManager>().GetAddress(id);
      var css = UnitOfWork<UserProfileManager>().GetCountryListForSelect(adr.Country);
      var countries = UnitOfWork<UserProfileManager>().GetCountryList();
      var cs = css.SingleOrDefault(c => adr.Country != null && countries.Any(r => r.Region == c.Value));
      if (cs != null) {
        cs.Selected = true;
      }
      return PartialView("Profile/Address/_EditAddress", adr);
    }

    [HttpPost]
    public JsonResult EditAddress(AddressBook adr, int countryId, string region) {
      UnitOfWork<UserProfileManager>().SetOrAddAddressForUser(adr, countryId, region, UserName);
      return Json(new { msg = "Saved" });
    }

    public JsonResult DeleteAddress(int id) {
      try {
        UnitOfWork<UserProfileManager>().DeleteAddress(id, UserName);
        return Json(new { msg = "OK" }, JsonRequestBehavior.AllowGet);
      } catch (Exception ex) {
        return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
      }
    }


    # endregion --== Addresses ==--

    # region Logon Externally

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [FeatureSet("texxtoor")]
    public ActionResult ExternalLogin(string provider, string returnUrl) {
      return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
    }

    private ActionResult RedirectToLocal(string returnUrl) {
      if (Url.IsLocalUrl(returnUrl)) {
        return Redirect(returnUrl);
      }
      return RedirectToAction("Index", "Home");
    }


    [AllowAnonymous]
    [FeatureSet("texxtoor")]
    public async Task<ActionResult> ExternalLoginCallback(string returnUrl) {
      var loginInfo = await UnitOfWork<UserManager>().AuthManager.GetExternalLoginInfoAsync();
      if (loginInfo == null) {
        return RedirectToAction("LogOn");
      }
      // Sign in the user with this external login provider if the user already has a login
      var user = await UnitOfWork<UserManager>().Usermanager.FindAsync(loginInfo.Login);
      if (user != null) {
        await UnitOfWork<UserManager>().SignInAsync(user, false);
        UnitOfWork<UserManager>().LogSignIn(user.UserName, false);
        return RedirectToLocal(returnUrl);
      } else {
        // If the user does not have an account, then prompt the user to create an account
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
        return View("External/ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
      }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl) {
      if (User.Identity.IsAuthenticated) {
        return RedirectToAction("Manage");
      }

      if (ModelState.IsValid) {
        // Get the information about the user from the external login provider
        var info = await UnitOfWork<UserManager>().AuthManager.GetExternalLoginInfoAsync();
        if (info == null) {
          return View("External/ExternalLoginFailure");
        }
        IdentityResult result = new IdentityResult();
        if (!String.IsNullOrEmpty(model.Password)) {
          var checkExistingUserWithPassword = UnitOfWork<UserManager>().Usermanager.FindByName(model.UserName);
          var hashedPassword = UnitOfWork<UserManager>().Usermanager.PasswordHasher.HashPassword(model.Password);
          if (UnitOfWork<UserManager>().Usermanager.PasswordHasher.VerifyHashedPassword(hashedPassword, model.Password) == PasswordVerificationResult.Success) {
            // an internal user exists and this external login will be associated with this if the user has the password
            result = await UnitOfWork<UserManager>().Usermanager.AddLoginAsync(checkExistingUserWithPassword.Id, info.Login);
          } else {
            // an internal user exists but the user does not provide the password
            UnitOfWork<UserManager>().RegisterFailedAttempt(checkExistingUserWithPassword.UserName);
            AddErrors(new IdentityResult(new string[] { "The password provided does not match the records. Please try again." }));
          }
        } else {
          // create complete new external account
          var user = new User() { UserName = model.UserName };
          result = await UnitOfWork<UserManager>().Usermanager.CreateAsync(user);
          if (result.Succeeded) {
            result = await UnitOfWork<UserManager>().Usermanager.AddLoginAsync(user.Id, info.Login);
            if (result.Succeeded) {
              await UnitOfWork<UserManager>().SignInAsync(user, isPersistent: false);
              return RedirectToLocal(returnUrl);
            }
          }
          AddErrors(result);
        }
      }
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      ViewBag.ReturnUrl = returnUrl;
      return View("External/ExternalLoginConfirmation", model);
    }

    //
    // GET: /Account/ExternalLoginFailure

    [AllowAnonymous]
    [FeatureSet("texxtoor")]
    public ActionResult ExternalLoginFailure() {
      return View("External/ExternalLoginFailure");
    }

    [AllowAnonymous]
    [ChildActionOnly]
    [FeatureSet("texxtoor")]
    public ActionResult ExternalLoginsList(string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      return PartialView("External/_ExternalLoginsListPartial", HttpContext.GetOwinContext().Authentication.GetExternalAuthenticationTypes());
    }

    [ChildActionOnly]
    [FeatureSet("texxtoor")]
    public ActionResult RemoveExternalLogins() {
      var linkedAccounts = UnitOfWork<UserManager>().Usermanager.GetLogins(Int32.Parse(User.Identity.GetUserId()));
      ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
      return PartialView("External/_RemoveExternalLoginsPartial", linkedAccounts);
    }

    [ChildActionOnly]
    public ActionResult RemoveAccountList() {
      var linkedAccounts = UnitOfWork<UserManager>().Usermanager.GetLogins(Int32.Parse(User.Identity.GetUserId()));
      ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
      return (ActionResult)PartialView("External/_RemoveExternalLoginsPartial", linkedAccounts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [FeatureSet("texxtoor")]
    public async Task<ActionResult> Manage(ChangePassword model) {
      bool hasPassword = HasPassword();
      ViewBag.HasLocalPassword = hasPassword;
      ViewBag.ReturnUrl = Url.Action("Manage");
      if (hasPassword) {
        if (ModelState.IsValid) {
          IdentityResult result = await UnitOfWork<UserManager>().Usermanager.ChangePasswordAsync(Int32.Parse(User.Identity.GetUserId()), model.OldPassword, model.NewPassword);
          if (result.Succeeded) {
            return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
          } else {
            AddErrors(result);
          }
        }
      } else {
        // User does not have a password so remove any validation errors caused by a missing OldPassword field
        ModelState state = ModelState["OldPassword"];
        if (state != null) {
          state.Errors.Clear();
        }

        if (ModelState.IsValid) {
          IdentityResult result = await UnitOfWork<UserManager>().Usermanager.AddPasswordAsync(Int32.Parse(User.Identity.GetUserId()), model.NewPassword);
          if (result.Succeeded) {
            return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
          } else {
            AddErrors(result);
          }
        }
      }

      // If we got this far, something failed, redisplay form
      return View("External/Manage", model);
    }

    private void AddErrors(IdentityResult result) {
      foreach (var error in result.Errors) {
        ModelState.AddModelError("", error);
      }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [FeatureSet("texxtoor")]
    public async Task<ActionResult> Disassociate(string provider, string providerUserId) {
      ManageMessageId? message = null;
      IdentityResult result = await UnitOfWork<UserManager>().Usermanager.RemoveLoginAsync(Int32.Parse(User.Identity.GetUserId()), new UserLoginInfo(provider, providerUserId));
      if (result.Succeeded) {
        message = ManageMessageId.RemoveLoginSuccess;
      } else {
        message = ManageMessageId.Error;
      }
      UnitOfWork<UserManager>().DisconnectUser(providerUserId, provider);
      return RedirectToAction("Manage", new { Message = message });
      //return RedirectToAction("Index","Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult LinkLogin(string provider) {
      // Request a redirect to the external login provider to link a login for the current user
      return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
    }

    //
    // GET: /Account/LinkLoginCallback
    public async Task<ActionResult> LinkLoginCallback() {
      var loginInfo = await UnitOfWork<UserManager>().AuthManager.GetExternalLoginInfoAsync(ChallengeResult.XsrfKey, User.Identity.GetUserId());
      if (loginInfo == null) {
        return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
      }
      var result = await UnitOfWork<UserManager>().Usermanager.AddLoginAsync(Int32.Parse(User.Identity.GetUserId()), loginInfo.Login);
      if (result.Succeeded) {
        return RedirectToAction("Manage");
      }
      return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
    }

    public ActionResult Manage(ManageMessageId? message) {
      ViewBag.StatusMessage =
        message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
          : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
              : message == ManageMessageId.Error ? "An error has occurred."
                : "";
      ViewBag.HasLocalPassword = HasPassword();
      ViewBag.ReturnUrl = Url.Action("Manage");
      return View("External/Manage");
    }

    private bool HasPassword() {
      var user = UnitOfWork<UserManager>().Usermanager.FindById(Int32.Parse(User.Identity.GetUserId()));
      if (user != null) {
        return user.PasswordHash != null;
      }
      return false;
    }


    # endregion

    #region IsEmail

    private bool IsEmail(string email) {
      return Regex.IsMatch(email, "^((([\\w]+\\.[\\w]+)+)|([\\w]+))@(([\\w]+\\.)+)([A-Za-z]{1,3})$");
    }

    #endregion

    [Authorize]
    public ActionResult Tasks() {
      // gather all tasks a user currently is responsible to:
      /* 
       *  1. List Texts he's assigned to
       *  2. List Teams he's member, pending, or with unconfirmed shares
       *  3. List milestones he's responsible for, with state
       *  4. List open invoices
       *  5. List projects with due actions if he's team lead     
       * 
       * */
      var projects = ProjectManager.Instance.GetProjectsWhereUserIsMember(UserName);
      var texts = projects.SelectMany(p => p.Opuses).ToList().Where(o => !o.IsPublished);
      var milestonesDue = texts.SelectMany(o => o.Milestones).Where(m => m.Overdue && m.Owner.Member.UserName == UserName);
      var milestonesNotDone = texts.SelectMany(o => o.Milestones).Where(m => !m.Overdue && m.Progress < 100 && m.Owner.Member.UserName == UserName);
      var teams = ProjectManager.Instance.GetTeamMemberships(UserName);
      var membershipsPending = teams.SelectMany(t => t.Members).Where(m => m.Member.UserName == UserName && m.Pending);
      var invoices = AccountingManager.Instance.GetInvoices(false, UserName).Where(i => !i.Paid);
      var vm = new Tasks
      {
        Projects = projects.ToList(),
        MilestonesDue =  milestonesDue.ToList(),
        MilestonesNotDone = milestonesNotDone.ToList(),
        TeamMemberPending = membershipsPending.ToList(),
        Texts = texts.ToList(),
        Invoices = invoices.ToList()
      };
      return View(vm);
    }

  }
}

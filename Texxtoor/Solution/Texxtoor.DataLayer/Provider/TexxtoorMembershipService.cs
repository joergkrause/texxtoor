using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.BaseLibrary.Services {

    public static class TexxtoorMembershipService {

        static TexxtoorMembershipService() {
            EnablePasswordReset = true;
            EnablePasswordRetrieval = true;
            HashAlgorithmType = "SHA1";
            MaxInvalidPasswordAttempts = 5;
            MinRequiredNonAlphanumericCharacters = 1;
            PasswordAttemptWindow = 5;
            MinRequiredPasswordLength = 6;
            PasswordStrengthRegularExpression = ".*";
            RequiresQuestionAndAnswer = false;
            UserIsOnlineTimeWindow = 60;
        }

        public static string ApplicationName {
            get;
            set;
        }

        public static bool EnablePasswordReset {
            get;
            private set;
        }

        public static bool EnablePasswordRetrieval {
            get;
            private set;
        }

        /// <summary>
        /// MD5, SHA1
        /// </summary>
        public static string HashAlgorithmType {
            get;
            private set;
        }
        //
        // Summary:
        //     Gets the number of invalid password or password-answer attempts allowed before
        //     the membership user is locked out.
        //
        // Returns:
        //     The number of invalid password or password-answer attempts allowed before
        //     the membership user is locked out.
        public static int MaxInvalidPasswordAttempts {
            get;
            private set;
        }
        //
        // Summary:
        //     Gets the minimum number of special characters that must be present in a valid
        //     password.
        //
        // Returns:
        //     The minimum number of special characters that must be present in a valid
        //     password.
        public static int MinRequiredNonAlphanumericCharacters {
            get;
            private set;
        }
        //
        // Summary:
        //     Gets the minimum length required for a password.
        //
        // Returns:
        //     The minimum length required for a password.
        public static int MinRequiredPasswordLength { get; private set; }
        //
        // Summary:
        //     Gets the time window between which consecutive failed attempts to provide
        //     a valid password or password answer are tracked.
        //
        // Returns:
        //     The time window, in minutes, during which consecutive failed attempts to
        //     provide a valid password or password answer are tracked. The default is 10
        //     minutes. If the interval between the current failed attempt and the last
        //     failed attempt is greater than the System.Web.Security.Membership.PasswordAttemptWindow
        //     property setting, each failed attempt is treated as if it were the first
        //     failed attempt.
        public static int PasswordAttemptWindow { get; private set; }
        //
        // Summary:
        //     Gets the regular expression used to evaluate a password.
        //
        // Returns:
        //     A regular expression used to evaluate a password.
        public static string PasswordStrengthRegularExpression { get; private set; }
        //
        //
        // Summary:
        //     Gets a value indicating whether the default membership provider requires
        //     the user to answer a password question for password reset and retrieval.
        //
        // Returns:
        //     true if a password answer is required for password reset and retrieval; otherwise,
        //     false.
        public static bool RequiresQuestionAndAnswer { get; private set; }
        //
        // Summary:
        //     Specifies the number of minutes after the last-activity date/time stamp for
        //     a user during which the user is considered online.
        //
        // Returns:
        //     The number of minutes after the last-activity date/time stamp for a user
        //     during which the user is considered online.
        public static int UserIsOnlineTimeWindow { get; private set; }

        public static string CreateHash(string password, string algorithm) {
            HashAlgorithm ha = null;
            byte[] pwBytes = Encoding.ASCII.GetBytes(password);
            switch (algorithm) {
                case "MD5":
                    ha = new MD5CryptoServiceProvider();
                    break;
                case "SHA1":
                    ha = new SHA1CryptoServiceProvider();
                    break;
                case "SHA256":
                    ha = new SHA256CryptoServiceProvider();
                    break;
                default:
                    throw new NotSupportedException("algorithm  not supported");
            }
            ha.ComputeHash(pwBytes);
            var sb = new StringBuilder();
            ha.Hash.ToList().ForEach(b => sb.Append(b));
            return sb.ToString();
        }

        public static MembershipUser CreateUser(User user, string providerName, object providerUserKey) {
            var mu = new MembershipUser(
                    providerName,
                    user.UserName,
                    providerUserKey,
                    user.Email,
                    user.PasswordQuestion,
                    user.PasswordAnswer,
                    user.IsApproved,
                    false,
                    user.CreatedAt,
                    user.LastLoginDate,
                    user.LastActivityDate,
                    user.LastPasswordChangedDate,
                    user.LastLockoutDate);
            return mu;            
        }

        private static readonly char[] punctuations = new char[] { '.', '?', '-', '/', ':' };

        public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters) {
            string str;
            int num;
            if ((length < 1) || (length > 0x80)) {
                throw new ArgumentException("Membership_password_length_incorrect");
            }
            if ((numberOfNonAlphanumericCharacters > length) || (numberOfNonAlphanumericCharacters < 0)) {
                throw new ArgumentException("Membership_min_required_non_alphanumeric_characters_incorrect", "numberOfNonAlphanumericCharacters");
            }
            do {
                var data = new byte[length];
                var chArray = new char[length];
                var num2 = 0;
                new RNGCryptoServiceProvider().GetBytes(data);
                for (int i = 0; i < length; i++) {
                    int num4 = data[i] % 0x57;
                    if (num4 < 10) {
                        chArray[i] = (char)(0x30 + num4);
                    } else if (num4 < 0x24) {
                        chArray[i] = (char)((0x41 + num4) - 10);
                    } else if (num4 < 0x3e) {
                        chArray[i] = (char)((0x61 + num4) - 0x24);
                    } else {
                        chArray[i] = punctuations[num4 - 0x3e];
                        num2++;
                    }
                }
                if (num2 < numberOfNonAlphanumericCharacters) {
                    var random = new Random();
                    for (int j = 0; j < (numberOfNonAlphanumericCharacters - num2); j++) {
                        int num6;
                        do {
                            num6 = random.Next(0, length);
                        }
                        while (!char.IsLetterOrDigit(chArray[num6]));
                        chArray[num6] = punctuations[random.Next(0, punctuations.Length)];
                    }
                }
                str = new string(chArray);
            }
            while (IsDangerousString(str, out num));
            return str;
        }

        private static char[] startingChars = new char[] { '<', '&' };

        private static bool IsDangerousString(string s, out int matchIndex) {
            matchIndex = 0;
            int startIndex = 0;
            while (true) {
                int num2 = s.IndexOfAny(startingChars, startIndex);
                if (num2 < 0) {
                    return false;
                }
                if (num2 == (s.Length - 1)) {
                    return false;
                }
                matchIndex = num2;
                char ch = s[num2];
                if (ch != '&') {
                    if ((ch == '<') && ((IsAtoZ(s[num2 + 1]) || (s[num2 + 1] == '!')) || ((s[num2 + 1] == '/') || (s[num2 + 1] == '?')))) {
                        return true;
                    }
                } else if (s[num2 + 1] == '#') {
                    return true;
                }
                startIndex = num2 + 1;
            }
        }

        private static bool IsAtoZ(char c) {
            return (((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')));
        }

        public static string GetCurrentUserName() {
            if (HostingEnvironment.IsHosted) {
                HttpContext current = HttpContext.Current;
                if (current != null) {
                    return current.User.Identity.Name;
                }
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if ((currentPrincipal != null) && (currentPrincipal.Identity != null)) {
                return currentPrincipal.Identity.Name;
            }
            return string.Empty;
        }


      public static string ErrorCodeToString(MembershipCreateStatus createStatus) {
        // See http://go.microsoft.com/fwlink/?LinkID=177550 for
        // a full list of status codes.
        switch (createStatus) {
          case MembershipCreateStatus.DuplicateUserName:
            return ControllerResources.AccountValidation_ErrorCodeToString_Username_already_exists;

          case MembershipCreateStatus.DuplicateEmail:
            return
              ControllerResources.AccountValidation_ErrorCodeToString_A_username_for_that_e_mail_address_already_exists;

          case MembershipCreateStatus.InvalidPassword:
            return ControllerResources.AccountValidation_ErrorCodeToString_The_password_provided_is_invalid;

          case MembershipCreateStatus.InvalidEmail:
            return ControllerResources.AccountValidation_ErrorCodeToString_The_e_mail_address_provided_is_invalid;

          case MembershipCreateStatus.InvalidAnswer:
            return
              ControllerResources.AccountValidation_ErrorCodeToString_The_password_retrieval_answer_provided_is_invalid;

          case MembershipCreateStatus.InvalidQuestion:
            return
              ControllerResources
                .AccountValidation_ErrorCodeToString_The_password_retrieval_question_provided_is_invalid;

          case MembershipCreateStatus.InvalidUserName:
            return ControllerResources.AccountValidation_ErrorCodeToString_The_user_name_provided_is_invalid;

          case MembershipCreateStatus.ProviderError:
            return ControllerResources.AccountValidation_ErrorCodeToString_The_authentication_provider_returned_an_error;

          case MembershipCreateStatus.UserRejected:
            return ControllerResources.AccountValidation_ErrorCodeToString_The_user_creation_request_has_been_canceled;

          default:
            return ControllerResources.AccountValidation_ErrorCodeToString_An_unknown_error_occurred;
        }
      }


      public static string ErrorCodesToString(IEnumerable<string> enumerable) {
        return String.Join(", ", enumerable);
      }
    }
}
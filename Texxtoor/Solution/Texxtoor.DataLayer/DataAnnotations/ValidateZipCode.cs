using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Validation.Attributes {
  public class ValidateZipCode : ValidationAttribute {

    private Dictionary<string, string> regex = new Dictionary<string, string> {
      {"UNITED STATES", @"^\d{5}([\-]?\d{4})?$"},
      {"UNITED KINGDOM", @"^(GIR|[A-Z]\d[A-Z\d]??|[A-Z]{2}\d[A-Z\d]??)[ ]??(\d[A-Z]{2})$"},
      {"GERMANY", @"\b((?:0[1-46-9]\d{3})|(?:[1-357-9]\d{4})|(?:[4][0-24-9]\d{3})|(?:[6][013-9]\d{3}))\b"},
      {"CANADA", @"^([ABCEGHJKLMNPRSTVXY]\d[ABCEGHJKLMNPRSTVWXYZ])\ {0,1}(\d[ABCEGHJKLMNPRSTVWXYZ]\d)$"},
      {"FRANCE", @"^(F-)?((2[A|B])|[0-9]{2})[0-9]{3}$"},
      {"ITALY", @"^(V-|I-)?[0-9]{5}$"},
      {"AUSTRALIA", @"^(0[289][0-9]{2})|([1345689][0-9]{3})|(2[0-8][0-9]{2})|(290[0-9])|(291[0-4])|(7[0-4][0-9]{2})|(7[8-9][0-9]{2})$"},
      {"NETHERLANDS", @"^[1-9][0-9]{3}\s?([a-zA-Z]{2})?$"},
      {"SPAIN", @"^([1-9]{2}|[0-9][1-9]|[1-9][0-9])[0-9]{3}$"},
      {"DENMARK", @"^([D-d][K-k])?( |-)?[1-9]{1}[0-9]{3}$"},
      {"SWEDEN", @"^(s-|S-){0,1}[0-9]{3}\s?[0-9]{2}$"},
      {"BELGIUM", @"^[1-9]{1}[0-9]{3}$"}
    };

    protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
      var model = validationContext.ObjectInstance as AddressBook;
      if (model != null && value != null) {
        if (model.Country != null && regex.ContainsKey(model.Country.ToUpper())) {
          var rx = regex[model.Country.ToUpper()];
          var check = new Regex(rx, RegexOptions.IgnoreCase | RegexOptions.Compiled);
          return check.IsMatch(value as string) ? ValidationResult.Success : new ValidationResult("Incorrect ZIP code for country " + model.Country);
        }
      }
      return ValidationResult.Success;
    }

    public override bool RequiresValidationContext {
      get {
        return true;
      }
    }

  }
}

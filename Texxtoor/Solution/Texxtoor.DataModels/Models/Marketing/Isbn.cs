using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Texxtoor.DataModels.ViewModels.Content {

  [ComplexType]
  public class Isbn {

    private readonly ISBN13 _isbnValidation = new ISBN13();

    private string _isbn13;

    [StringLength(20)]
    public string Isbn13 {
      get { return _isbn13; }
      set {
        _isbnValidation.ISBN = value;
        if (_isbnValidation.IsValid) {
          _isbn13 = value;
        }
      } 
    }

    [StringLength(18)]
    public string Isbn10 { get; set; }

    public bool Claimed { get; set; }


    public bool ValidateIsbn(string isbn) {
      var validator = new ISBN13(isbn);
      return validator.IsValid;
    }

  }

  public enum ISBNGroupingFormat {
    Ungrouped,
    Grouped,
    Invalid
  }

  public class GroupingFormatException : Exception {
    public override string Message {
      get {
        return "There is no support jet to convert Ungrouped ISBN to a group ISBN.";
      }
    }
  }

  public class ISBN13 {
    #region constructor
    public ISBN13() {
      _originalISBN = "";
      _isbn = "";
    }
    public ISBN13(string isbn) {
      _isbn = isbn;
      _originalISBN = isbn;
    }
    #endregion

    #region fields
    string _originalISBN = null;
    const string _isbnGroupReg = "^((ISBN)?\\s?)(?=[- 0-9]{17}$)(?:([0-9]{3})[- ]{1}((?=[- 0-9]{11})(?:([0-9]{1,5})[- ]{1}([0-9]{1,6})[- ]{1}([0-9]{1,6})))[- ]{1}[0-9]{1}$)";
    const string _isbnUngroupReg = "^(?=[0-9]{13})(?:[0-9]{3}(([0-5]{1}|[7-9]{1})[0-9]{4})[0-9]{5}$)";
    const string _isbnToUngroupedFormat = "(ISBN)?[- ]?";
    #endregion

    #region properties
    private ISBNGroupingFormat _groupingFormat;
    /// <summary>
    /// Get or set the way the ISBN is formated.
    /// </summary>
    /// <example>
    /// Grouped format:     978-0-571-08989-5
    /// Ungrouped fromat:   9780571089895
    /// </example>
    public ISBNGroupingFormat GroupingFormat {
      get { return _groupingFormat; }
      set {
        if (_groupingFormat == ISBNGroupingFormat.Ungrouped && value == ISBNGroupingFormat.Grouped && GetISBNFormat(_originalISBN) == ISBNGroupingFormat.Ungrouped) {
          throw new GroupingFormatException();
        } else if (_groupingFormat == ISBNGroupingFormat.Grouped && value == ISBNGroupingFormat.Ungrouped) {
          _groupingFormat = value;
          _isbn = Regex.Replace(_isbn, _isbnToUngroupedFormat, "");
        }
      }
    }

    private string _isbn;
    /// <summary>
    /// Get or set the ISBN number.
    /// </summary>
    public string ISBN {
      get { return _isbn; }
      set {
        var cleanValue = value.Trim();

        _groupingFormat = GetISBNFormat(cleanValue);
        _originalISBN = cleanValue;
        _isbn = cleanValue;
      }
    }

    /// <summary>
    /// Get true if the ISBN is a valid number.
    /// </summary>
    public bool IsValid {
      get { return IsValidISBN(_isbn); }
    }
    #endregion

    #region static methods
    /// <summary>
    /// Validate if the a ISBN is in valid Human Readable format. It also checks if the checksum is correct.
    /// </summary>
    /// <param name="isbn">ISBN</param>
    /// <returns>
    /// Return true if the format is correct and the ISBN checksum is valid. 
    /// Return false if the format is incorrect and the ISBN checksum is invalid.
    /// </returns>
    public static bool IsValidISBN(string isbn) {
      switch (GetISBNFormat(isbn)) {
        case ISBNGroupingFormat.Ungrouped:
          return ValidatedChecksum(isbn);
        case ISBNGroupingFormat.Grouped:
          return ValidatedChecksum(Regex.Replace(isbn, "(ISBN)?[- ]?", "").Trim());
        case ISBNGroupingFormat.Invalid:
          return false;
        default:
          return false;
      }
    }

    /// <summary>
    /// Get the current type of format.
    /// </summary>
    /// <param name="isbn">ISBN</param>
    /// <returns>Returns the type of format the given ISBN is in.</returns>
    public static ISBNGroupingFormat GetISBNFormat(string isbn) {
      if (Regex.IsMatch(isbn, _isbnGroupReg)) {
        return ISBNGroupingFormat.Grouped;
      } else if (Regex.IsMatch(isbn, _isbnUngroupReg)) {
        return ISBNGroupingFormat.Ungrouped;
      } else {
        return ISBNGroupingFormat.Invalid;
      }
    }

    /// <summary>
    /// Check if the ISBN checksum is correct.
    /// </summary>
    /// <param name="isbn">ISBN</param>
    /// <returns>
    /// Return true when the number is correct and the checksum is correct.
    /// Return false when the number is incorrect and the checksum is incorrect.
    /// </returns>
    private static bool ValidatedChecksum(string isbn) {
      var workingIsbn = isbn;
      if (GetISBNFormat(isbn) == ISBNGroupingFormat.Grouped) {
        workingIsbn = Regex.Replace(isbn, _isbnToUngroupedFormat, "");
      }
      Debug.WriteLine("_re = " + _isbnUngroupReg);
      if (Regex.IsMatch(workingIsbn, _isbnUngroupReg)) {
        var sumOfProducts = 0;

        var multiply = 1;
        for (var i = 0; i < 12; i++) {
          var productValue = int.Parse(workingIsbn.Substring(i, 1)); //single char

          //in turns the value are multiplied with 1 or 3.
          if (multiply == 1) {
            sumOfProducts += (productValue * multiply); //added to sumOfProducts
            multiply = 3;
          } else if (multiply == 3) {
            sumOfProducts += (productValue * multiply); //added to sumOfProducts
            multiply = 1;
          }
        }
        var mod = sumOfProducts % 10; //calculate value
        var checkSum = 10 - mod;

        return (isbn.Substring(12) == checkSum.ToString()); //ISBN is valid or invalid.
      } else {
        return false; //numbers 4-8 not in de range 00000-59999 or 70000-99999
      }
    }
    #endregion
  }

}

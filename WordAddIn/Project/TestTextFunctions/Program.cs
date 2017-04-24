using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TestTextFunctions {
  class Program {
    static void Main(string[] args) {

      var f = File.ReadAllText(@"E:\Temp\Handbuch.docx\word-html-step-0.html");
      var r = Quote(f);
      File.WriteAllText(@"E:\Temp\Handbuch.docx\word-html-step-quotes.html", r);
    }

    /*‘ (U+2018) LEFT SINGLE QUOTATION MARK               8216
      ’ (U+2019) RIGHT SINGLE QUOTATION MARK              8217
      “ (U+201C) LEFT DOUBLE QUOTATION MARK               8220
      ” (U+201D) RIGHT DOUBLE QUOTATION MARK              8221
      << 00AB LEFT-POINTING DOUBLE ANGLE QUOTATION MARK    171
      >> 00BB RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK   187
      <  2039 LEFT-POINTING SINGLE ANGLE QUOTATION MARK   8249
      >  203A RIGHT-POINTING SINGLE ANGLE QUOTATION MARK  8250
      -  2013 EN DASH                                     8093 UTF 8211 HTML
      -  2014 EM DASH                                     8094 UTF 8212 HTML
      -  201E DOUBLE LOW-9 QUOTATION MARK                 809E UTF 8212 HTML
      -  201C LEFT DOUBLE QUOTATION MARK                  809C UTF 8212 HTML
      -  201D RIGHT DOUBLE QUOTATION MARK                 809D UTF 8212 HTML
     * 
     * Word use for German formatting DOUBLE LOW-9 LEFT and LEFT DOUBLE to the right
     * 
     */

    public static string Quote(string content) {
      // first: check that quotes match
      var replacements = new Dictionary<char, string>();
      replacements.Add('\u2013', "&#8211;"); // add more if needed      
      replacements.Add('\u2014', "&#8212;");  
      var pairs = new List<KeyValuePair<char, string>> {
        new KeyValuePair<char, string>('\u201E', "<q class='double'>"),
        new KeyValuePair<char, string>('\u201C', "</q>"),
        new KeyValuePair<char, string>('\u201C', "<q class='double'>"),
        new KeyValuePair<char, string>('\u201D', "</q>"),
        new KeyValuePair<char, string>('\u2018', "<q class='single'>"),
        new KeyValuePair<char, string>('\u2019', "</q>")
      };
      var replacePairs = new List<KeyValuePair<char, string>>();
      for (int i = 0; i < pairs.Count; i+=2) {
        if (content.Count(c => c == pairs[i].Key) == content.Count(c => c == pairs[i + 1].Key)) {
          // pair matches, add to replace list
          replacePairs.Add(pairs[i]);
          replacePairs.Add(pairs[i+1]);
        }
      }
      content = replacePairs.Aggregate(content, (current, replacePair) => current.Replace(replacePair.Key.ToString(CultureInfo.InvariantCulture), replacePair.Value));
      return replacements.Aggregate(content, (current, dash) => current.Replace(dash.Key.ToString(CultureInfo.InvariantCulture), dash.Value));
    }


  }
}

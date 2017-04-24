using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core.Extensions {
  public static class StringExtension {

    private static Dictionary<string, int> entities = new Dictionary<string, int>();
    private static Dictionary<string, int> entitiesLight = new Dictionary<string, int>();
    private static Dictionary<string, string> entitiesDecode = new Dictionary<string, string>();

    private static void CreateEntityTableLight() {
      entitiesLight.Add("&nbsp;", 160);
      entitiesLight.Add("&amp;", 38);
      entitiesLight.Add("&quot;", 34);
      entitiesLight.Add("&lt;", 60);
      entitiesLight.Add("&gt;", 62);
    }

    private static void CreateEntityTableDecode() {
      entitiesDecode.Add("&nbsp;", " ");
      entitiesDecode.Add("&amp;", "&");
      entitiesDecode.Add("&quot;", "\"");
      entitiesDecode.Add("&#160;", " ");
      entitiesDecode.Add("&#38;", "&");
      entitiesDecode.Add("&#34;", "\"");
    }

    private static void CreateEntityTable() {
      entities.Add("&nbsp;", 160);
      entities.Add("&amp;", 38);
      entities.Add("&quot;", 34);
      entities.Add("&cent;", 162);
      entities.Add("&euro;", 8364);
      entities.Add("&pound;", 163);
      entities.Add("&yen;", 165);
      entities.Add("&copy;", 169);
      entities.Add("&reg;", 174);
      entities.Add("&trade;", 8482);
      entities.Add("&permil;", 8240);
      entities.Add("&micro;", 181);
      entities.Add("&middot;", 183);
      entities.Add("&bull;", 8226);
      entities.Add("&hellip;", 8230);
      entities.Add("&prime;", 8242);
      entities.Add("&Prime;", 8243);
      entities.Add("&sect;", 167);
      entities.Add("&para;", 182);
      entities.Add("&szlig;", 223);
      entities.Add("&lsaquo;", 8249);
      entities.Add("&rsaquo;", 8250);
      entities.Add("&laquo;", 171);
      entities.Add("&raquo;", 187);
      entities.Add("&lsquo;", 8216);
      entities.Add("&rsquo;", 8217);
      entities.Add("&ldquo;", 8220);
      entities.Add("&rdquo;", 8221);
      entities.Add("&sbquo;", 8218);
      entities.Add("&bdquo;", 8222);
      entities.Add("&lt;", 60);
      entities.Add("&gt;", 62);
      entities.Add("&le;", 8804);
      entities.Add("&ge;", 8805);
      entities.Add("&ndash;", 8211);
      entities.Add("&mdash;", 8212);
      entities.Add("&macr;", 175);
      entities.Add("&oline;", 8254);
      entities.Add("&curren;", 164);
      entities.Add("&brvbar;", 166);
      entities.Add("&uml;", 168);
      entities.Add("&iexcl;", 161);
      entities.Add("&iquest;", 191);
      entities.Add("&circ;", 710);
      entities.Add("&tilde;", 732);
      entities.Add("&deg;", 176);
      entities.Add("&minus;", 8722);
      entities.Add("&plusmn;", 177);
      entities.Add("&divide;", 247);
      entities.Add("&frasl;", 8260);
      entities.Add("&times;", 215);
      entities.Add("&sup1;", 185);
      entities.Add("&sup2;", 178);
      entities.Add("&sup3;", 179);
      entities.Add("&frac14;", 188);
      entities.Add("&frac12;", 189);
      entities.Add("&frac34;", 190);

      entities.Add("&fnof;", 402);
      entities.Add("&int;", 8747);
      entities.Add("&sum;", 8721);
      entities.Add("&infin;", 8734);
      entities.Add("&radic;", 8730);
      entities.Add("&sim;", 8764);
      entities.Add("&cong;", 8773);
      entities.Add("&asymp;", 8776);
      entities.Add("&ne;", 8800);
      entities.Add("&equiv;", 8801);
      entities.Add("&isin;", 8712);
      entities.Add("&notin;", 8713);
      entities.Add("&ni;", 8715);
      entities.Add("&prod;", 8719);
      entities.Add("&and;", 8743);
      entities.Add("&or;", 8744);
      entities.Add("&not;", 172);
      entities.Add("&cap;", 8745);
      entities.Add("&cup;", 8746);
      entities.Add("&part;", 8706);
      entities.Add("&forall;", 8704);
      entities.Add("&exist;", 8707);
      entities.Add("&empty;", 8709);
      entities.Add("&nabla;", 8711);
      entities.Add("&lowast;", 8727);
      entities.Add("&prop;", 8733);
      entities.Add("&ang;", 8736);

      entities.Add("&acute;", 180);
      entities.Add("&cedil;", 184);
      entities.Add("&ordf;", 170);
      entities.Add("&ordm;", 186);
      entities.Add("&dagger;", 8224);
      entities.Add("&Dagger;", 8225);

      entities.Add("&Agrave;", 192);
      entities.Add("&Aacute;", 193);
      entities.Add("&Acirc;", 194);
      entities.Add("&Atilde;", 195);
      entities.Add("&Auml;", 196);
      entities.Add("&Aring;", 197);
      entities.Add("&AElig;", 198);
      entities.Add("&Ccedil;", 199);
      entities.Add("&Egrave;", 200);
      entities.Add("&Eacute;", 201);
      entities.Add("&Ecirc;", 202);
      entities.Add("&Euml;", 203);
      entities.Add("&Igrave;", 204);
      entities.Add("&Iacute;", 205);
      entities.Add("&Icirc;", 206);
      entities.Add("&Iuml;", 207);
      entities.Add("&ETH;", 208);
      entities.Add("&Ntilde;", 209);
      entities.Add("&Ograve;", 210);
      entities.Add("&Oacute;", 211);
      entities.Add("&Ocirc;", 212);
      entities.Add("&Otilde;", 213);
      entities.Add("&Ouml;", 214);
      entities.Add("&Oslash;", 216);
      entities.Add("&OElig;", 338);
      entities.Add("&Scaron;", 352);
      entities.Add("&Ugrave;", 217);
      entities.Add("&Uacute;", 218);
      entities.Add("&Ucirc;", 219);
      entities.Add("&Uuml;", 220);
      entities.Add("&Yacute;", 221);
      entities.Add("&Yuml;", 376);
      entities.Add("&THORN;", 222);
      entities.Add("&agrave;", 224);
      entities.Add("&aacute;", 225);
      entities.Add("&acirc;", 226);
      entities.Add("&atilde;", 227);
      entities.Add("&auml;", 228);
      entities.Add("&aring;", 229);
      entities.Add("&aelig;", 230);
      entities.Add("&ccedil;", 231);
      entities.Add("&egrave;", 232);
      entities.Add("&eacute;", 233);
      entities.Add("&ecirc;", 234);
      entities.Add("&euml;", 235);
      entities.Add("&igrave;", 236);
      entities.Add("&iacute;", 237);
      entities.Add("&icirc;", 238);
      entities.Add("&iuml;", 239);
      entities.Add("&eth;", 240);
      entities.Add("&ntilde;", 241);
      entities.Add("&ograve;", 242);
      entities.Add("&oacute;", 243);
      entities.Add("&ocirc;", 244);
      entities.Add("&otilde;", 245);
      entities.Add("&ouml;", 246);
      entities.Add("&oslash;", 248);
      entities.Add("&oelig;", 339);
      entities.Add("&scaron;", 353);
      entities.Add("&ugrave;", 249);
      entities.Add("&uacute;", 250);
      entities.Add("&ucirc;", 251);
      entities.Add("&uuml;", 252);
      entities.Add("&yacute;", 253);
      entities.Add("&thorn;", 254);
      entities.Add("&yuml;", 255);
      entities.Add("&Alpha;", 913);
      entities.Add("&Beta;", 914);
      entities.Add("&Gamma;", 915);
      entities.Add("&Delta;", 916);
      entities.Add("&Epsilon;", 917);
      entities.Add("&Zeta;", 918);
      entities.Add("&Eta;", 919);
      entities.Add("&Theta;", 920);
      entities.Add("&Iota;", 921);
      entities.Add("&Kappa;", 922);
      entities.Add("&Lambda;", 923);
      entities.Add("&Mu;", 924);
      entities.Add("&Nu;", 925);
      entities.Add("&Xi;", 926);
      entities.Add("&Omicron;", 927);
      entities.Add("&Pi;", 928);
      entities.Add("&Rho;", 929);
      entities.Add("&Sigma;", 931);
      entities.Add("&Tau;", 932);
      entities.Add("&Upsilon;", 933);
      entities.Add("&Phi;", 934);
      entities.Add("&Chi;", 935);
      entities.Add("&Psi;", 936);
      entities.Add("&Omega;", 937);
      entities.Add("&alpha;", 945);
      entities.Add("&beta;", 946);
      entities.Add("&gamma;", 947);
      entities.Add("&delta;", 948);
      entities.Add("&epsilon;", 949);
      entities.Add("&zeta;", 950);
      entities.Add("&eta;", 951);
      entities.Add("&theta;", 952);
      entities.Add("&iota;", 953);
      entities.Add("&kappa;", 954);
      entities.Add("&lambda;", 955);
      entities.Add("&mu;", 956);
      entities.Add("&nu;", 957);
      entities.Add("&xi;", 958);
      entities.Add("&omicron;", 959);
      entities.Add("&pi;", 960);
      entities.Add("&rho;", 961);
      entities.Add("&sigmaf;", 962);
      entities.Add("&sigma;", 963);
      entities.Add("&tau;", 964);
      entities.Add("&upsilon;", 965);
      entities.Add("&phi;", 966);
      entities.Add("&chi;", 967);
      entities.Add("&psi;", 968);
      entities.Add("&omega;", 969);
      entities.Add("&alefsym;", 8501);
      entities.Add("&piv;", 982);
      entities.Add("&real;", 8476);
      entities.Add("&thetasym;", 977);
      entities.Add("&upsih;", 978);
      entities.Add("&weierp;", 8472);
      entities.Add("&image;", 8465);
      entities.Add("&larr;", 8592);
      entities.Add("&uarr;", 8593);
      entities.Add("&rarr;", 8594);
      entities.Add("&darr;", 8595);
      entities.Add("&harr;", 8596);
      entities.Add("&crarr;", 8629);
      entities.Add("&lArr;", 8656);
      entities.Add("&uArr;", 8657);
      entities.Add("&rArr;", 8658);
      entities.Add("&dArr;", 8659);
      entities.Add("&hArr;", 8660);
      entities.Add("&there4;", 8756);
      entities.Add("&sub;", 8834);
      entities.Add("&sup;", 8835);
      entities.Add("&nsub;", 8836);
      entities.Add("&sube;", 8838);
      entities.Add("&supe;", 8839);
      entities.Add("&oplus;", 8853);
      entities.Add("&otimes;", 8855);
      entities.Add("&perp;", 8869);
      entities.Add("&sdot;", 8901);
      entities.Add("&lceil;", 8968);
      entities.Add("&rceil;", 8969);
      entities.Add("&lfloor;", 8970);
      entities.Add("&rfloor;", 8971);
      entities.Add("&lang;", 9001);
      entities.Add("&rang;", 9002);
      entities.Add("&loz;", 9674);
      entities.Add("&spades;", 9824);
      entities.Add("&clubs;", 9827);
      entities.Add("&hearts;", 9829);
      entities.Add("&diams;", 9830);
      entities.Add("&ensp;", 8194);
      entities.Add("&emsp;", 8195);
      entities.Add("&thinsp;", 8201);
      entities.Add("&zwnj;", 8204);
      entities.Add("&zwj;", 8205);
      entities.Add("&lrm;", 8206);
      entities.Add("&rlm;", 8207);
      entities.Add("&shy;", 173);
    }

    [DebuggerStepThrough]
    public static string ToHtmlNumericEntity(this string html, bool light = false) {
      return HtmlEncodeSpecialChars(html, light);
    }

    [DebuggerStepThrough]
    public static string FromHtmlNumericEntity(this string html) {
      return HtmlDecodeSpecialChars(html);
    }

    private static readonly char utf16Bom = Encoding.Unicode.GetString(Encoding.Unicode.GetPreamble())[0];

    private static string HtmlEncodeSpecialChars(string text, bool light) {
      var sb = new StringBuilder();
      foreach (var c in text) {
        // remove bom's
        if (c == utf16Bom) continue;
        if (c > 159 && !light) // special chars
          sb.Append(String.Format("&#{0};", (int)c));
        else
          sb.Append(c);
      }
      // convert entities in numeric entities to avoid confusing XML processing
      if (!entities.Any()) {
        CreateEntityTable();
      }
      if (!entitiesLight.Any()) {
        CreateEntityTableLight();
      }
      if (light) {
        foreach (var entity in entitiesLight) {
          sb.Replace(entity.Key, String.Format("&#{0};", entity.Value));
        }
      }
      else {
        foreach (var entity in entities) {
          sb.Replace(entity.Key, String.Format("&#{0};", entity.Value));
        }
      }
      return sb.ToString();
    }

    private static string HtmlDecodeSpecialChars(string text) {
      // convert entities in numeric entities to avoid confusing XML processing
      if (!entitiesDecode.Any()) {
        CreateEntityTableDecode();
      }
      foreach (var entity in entitiesDecode) {
        text = text.Replace(entity.Key, entity.Value);
      }
      return text;
    }

    [DebuggerStepThrough]
    public static string ToCamelCase(this string text) {
      return text.Replace(" ", "");
    }

    [DebuggerStepThrough]
    public static string EncodeJsString(this string text) {
      return text;
    }

    [DebuggerStepThrough]
    public static IEnumerable<Enum> ToEnumerable(this Enum input) {
      return Enum.GetValues(input.GetType()).Cast<Enum>().Where(value => input.HasFlag(value) && Convert.ToInt64(value) != 0);
    }

    [DebuggerStepThrough]
    public static string CreatePath(this string basePath, string file) {
      return String.Format("{0}{1}{2}", basePath, (String.IsNullOrEmpty(basePath) ? "" : "/"), file);
    }

    [DebuggerStepThrough]
    public static bool ParseCheckValue(this string val) {
      if (val.StartsWith("true") || val.EndsWith("true") || val.StartsWith("false") || val.EndsWith("false")) {
        return Boolean.Parse(val.Contains(",") ? val.Split(',')[0] : val);
      }
      return false;
    }

    [DebuggerStepThrough]
    public static string InnerTrim(this string text) {
      return text.Replace(" ", "");
    }

    [DebuggerStepThrough]
    public static string NullSafe(this string target) {
      return (target ?? String.Empty).Trim();
    }

    [DebuggerStepThrough]
    public static string FormatWith(this string target, params object[] args) {
      return String.Format(CultureInfo.CurrentCulture, target, args);
    }

    [DebuggerStepThrough]
    public static MvcHtmlString Ellipsis(this string value, int maxLength) {
      return Ellipsis(value, maxLength, null);
    }

    /// <summary>
    /// Shorten string, trim, and strip tags
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string CleanUpString(this string value, int maxLength) {
      return Ellipsis(value, maxLength, null).ToHtmlString().Trim().StripTags();
    }

    [DebuggerStepThrough]
    public static byte[] GetBytes(this string value) {
      return Encoding.UTF8.GetBytes(value);
    }

    [DebuggerStepThrough]
    public static MvcHtmlString Ellipsis(this string value, int maxLength, string actionLink) {
      const string suffix = " ...";
      const string boundaryChars = " .,;";
      if (value == null)
        return null;
      string s = value.Replace("<![CDATA[", "").Replace("]]>", "");
      if (String.IsNullOrEmpty(s) || s.Length <= maxLength || s.Length < suffix.Length)
        return MvcHtmlString.Create(s);
      // try breaking at word boundary back from last character
      var sub = s.Substring(0, maxLength - suffix.Length);
      var idx = sub.Length;
      do { } while (!boundaryChars.Contains(s[--idx]) && idx > 0);
      return MvcHtmlString.Create(s.Substring(0, ++idx) + (String.IsNullOrEmpty(actionLink) ? suffix : actionLink));
    }

    [DebuggerStepThrough]
    public static void GetIntPair(this string res, out int w, out int h) {
      if (!String.IsNullOrEmpty(res) && res.Contains("x")) {
        w = Int32.Parse(res.Split('x')[0]);
        h = Int32.Parse(res.Split('x')[1]);
      } else {
        w = 0;
        h = 0;
      }
    }

    [DebuggerStepThrough]
    public static string StripTags(this string source) {
      char[] array = new char[source.Length];
      int arrayIndex = 0;
      bool inside = false;

      for (int i = 0; i < source.Length; i++) {
        char let = source[i];
        if (@let == '<') {
          inside = true;
          continue;
        }
        if (@let == '>') {
          inside = false;
          continue;
        }
        if (!inside) {
          array[arrayIndex] = @let;
          arrayIndex++;
        }
      }
      return new string(array, 0, arrayIndex);
    }


  }


}

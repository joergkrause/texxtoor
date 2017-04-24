using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;

namespace Texxtoor.BaseLibrary.Globalization
{
    /// <summary>
    /// Provides basic translation features via several Web interfaces
    /// 
    /// NOTE: These services may change their format or otherwise fail.
    /// </summary>
    public class TranslationServices
    {
        /// <summary>
        /// Error message set when an error occurs in the translation service
        /// </summary>
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        private string _ErrorMessage = "";

        /// <summary>
        /// Timeout for how long to wait for a translation
        /// </summary>
        public int TimeoutSeconds
        {
            get { return _TimeoutSeconds; }
            set { _TimeoutSeconds = value; }
        }
        private int _TimeoutSeconds = 10;


        /// <summary>
        /// Translates a string into another language using Google's Translation Pages.
        /// 
        /// &lt;&lt;a href=&quot;http://translate.google.com/translate_t&quot; 
        /// target=&quot;top&quot;&gt;&gt;http://translate.google.com/translate_t&lt;&l;
        /// t;/a&gt;&gt;
        /// <seealso>Class TranslationServices</seealso>
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="FromCulture">
        /// Two letter culture (en of en-us, fr of fr-ca, de of de-ch)
        /// </param>
        /// <param name="ToCulture">
        /// Two letter culture
        /// </param>
        public string TranslateGoogle(string text, string fromCulture, string toCulture)
        {
            fromCulture = fromCulture.ToLower();
            toCulture = toCulture.ToLower();

            // use web service

            string Result = "";
            
            return HttpUtility.HtmlDecode(Result);            
        }

        /// <summary>
        /// Translates a string using Yahoo's Babel fish service.
        /// 
        /// &lt;&lt;a href=&quot;http://babelfish.yahoo.com/&quot; 
        /// target=&quot;top&quot;&gt;&gt;http://babelfish.yahoo.com/&lt;&lt;/a&gt;&gt;
        /// <seealso>Class TranslationServices</seealso>
        /// </summary>
        /// <param name="Text">
        /// The text to translate
        /// </param>
        /// <param name="FromCulture">
        /// Two letter culture (en of en-us, fr of fr-ca, de of de-ch)
        /// </param>
        /// <param name="ToCulture">
        /// Two letter culture
        /// </param>
        public string TranslateBabelFish(string Text, string FromCulture, string ToCulture)
        {
            FromCulture = FromCulture.ToLower();
            ToCulture = ToCulture.ToLower();

            string Result = "";

            return HttpUtility.HtmlDecode(Result); 
        }


    }
}

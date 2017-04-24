using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.IO;

namespace Texxtoor.Portal.Services {
  /// <summary>
  /// Defines all translator service functions.
  /// </summary>
  public interface ITranslator {
    string Translate(string text, string fromCulture, string toCulture);
    string ErrorMessage { get; }
  }

  /// <summary>
  /// List of supported translators as per <see cref="TranslatorFactory"/>.
  /// </summary>
  public enum Translators {
    Google,
    Bing,
    Yahoo
  }

  /// <summary>
  /// Creates the translator object
  /// </summary>
  public static class TranslatorFactory {

    private static ITranslator google, bing, yahoo;

    public static ITranslator GetTranslator(Translators translator) {
      ITranslator requestedTranslator = null;
      switch (translator) {
        case Translators.Google:
          if (google == null)
            google = new GoogleTranslator();
          requestedTranslator = google;
          break;
        case Translators.Bing:
          if (bing == null)
            bing = new BingTranslator();
          requestedTranslator = bing;
          break;
        case Translators.Yahoo:
          if (yahoo == null)
            yahoo = new YahooTranslator();
          requestedTranslator = yahoo;
          break;
      }
      if (requestedTranslator == null)
        throw new NotSupportedException(translator + " is not supported");
      return requestedTranslator;
    }
  }

  public class GoogleTranslator : ITranslator {

    public string Translate(string text, string fromCulture, string toCulture) {
      fromCulture = fromCulture.ToLower();
      toCulture = toCulture.ToLower();

      // normalize the culture in case something like en-us was passed 
      // retrieve only en since Google doesn't support sub-locales
      string[] tokens = fromCulture.Split('-');
      if (tokens.Length > 1)
        fromCulture = tokens[0];

      // normalize ToCulture
      tokens = toCulture.Split('-');
      if (tokens.Length > 1)
        toCulture = tokens[0];

      string url = string.Format(@"http://translate.google.com/translate_a/t?client=j&text={0}&hl=en&sl={1}&tl={2}",
                                 HttpUtility.UrlEncode(text), fromCulture, toCulture);

      // Retrieve Translation with HTTP GET call
      string json = null;
      try {
        WebClient web = new WebClient();

        // MUST add a known browser user agent or else response encoding doen't return UTF-8 (WTF Google?)
        web.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
        web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

        // Make sure we have response encoding to UTF-8
        web.Encoding = Encoding.UTF8;
        json = web.DownloadString(url);
      } catch (Exception ex) {
        this.ErrorMessage = ex.Message;
        return null;
      }

      string result = null;
      // decode the JSON result
      var js = new System.Web.Script.Serialization.JavaScriptSerializer();
      var glossaryEntry = js.Deserialize(json, typeof(object)) as dynamic;
      result = glossaryEntry["sentences"][0]["trans"];
      if (!string.IsNullOrEmpty(result)) return result;
      ErrorMessage = "Empty Result";
      return null;
    }

    public string ErrorMessage {
      get;
      private set;
    }
  }

  public class BingTranslator : ITranslator {

    public string Translate(string text, string fromCulture, string toCulture) {
      var appId = WebConfigurationManager.AppSettings["social:BINGAppId"];
      var uri = String.Format("http://api.microsofttranslator.com/v2/Http.svc/Translate?appId={0}&text={1}&from={2}&to={3}",
          appId, text, fromCulture, toCulture);

      var translation = String.Empty;

      var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

      WebResponse response = null;
      try {
        response = httpWebRequest.GetResponse();
        using (var stream = response.GetResponseStream()) {
          // decode the service result
          var dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
          translation = (string)dcs.ReadObject(stream);
        }
      } catch (WebException e) {
        ProcessWebException(e, "Failed to translate");
      } finally {
        if (response != null) {
          response.Close();
          response = null;
        }
      }
      return translation;
    }

    private void ProcessWebException(WebException e, string message) {
      // Obtain detailed error information
      string strResponse = string.Empty;
      using (HttpWebResponse response = (HttpWebResponse)e.Response) {
        using (Stream responseStream = response.GetResponseStream()) {
          using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII)) {
            strResponse = sr.ReadToEnd();
          }
        }
      }
      ErrorMessage = strResponse;
    }

    public string ErrorMessage {
      get;
      private set;
    }
  }

  public class YahooTranslator : ITranslator {

    public string Translate(string text, string fromCulture, string toCulture) {
      throw new NotImplementedException();
    }

    public string ErrorMessage {
      get { throw new NotImplementedException(); }
    }
  }
}
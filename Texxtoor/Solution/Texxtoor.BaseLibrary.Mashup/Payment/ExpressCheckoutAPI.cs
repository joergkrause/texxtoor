using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Texxtoor.BaseLibrary.Mashup.Payment.ExpressCheckout {
  /// <summary>
  /// Summary description for NVPAPICaller
  /// </summary>
  public class NVPExpressAPICaller {
    //private static readonly ILog log = LogManager.GetLogger(typeof(NVPAPICaller));

    private readonly string pendpointurl = "https://api-3t.paypal.com/nvp";
    private readonly string sandboxapiurl = "https://api-3t.sandbox.paypal.com/nvp";

    //Flag that determines the PayPal environment (live or sandbox)
    private readonly bool _bSandbox = Boolean.Parse(WebConfigurationManager.AppSettings["paypal:UseSandbox"]);

    //Replace <API_USERNAME> with your API Username
    //Replace <API_PASSWORD> with your API Password
    //Replace <API_SIGNATURE> with your Signature
    private string APIUsername;
    private string APIPassword;
    private string APISignature;
    private string returnURL;
    private string cancelURL;
    private string Subject = "";
    private string BNCode = "PP-ECWizard";

    public NVPExpressAPICaller() {
    }


    public NVPExpressAPICaller(string returnURL, string cancelURL) {
      this.returnURL = returnURL;
      this.cancelURL = cancelURL;
    }

    //HttpWebRequest Timeout specified in milliseconds 
    private readonly int Timeout = Int32.Parse(WebConfigurationManager.AppSettings["paypal:Timeout"]);
    private static readonly string[] SECURED_NVPS = new string[] { "ACCT", "CVV2", "SIGNATURE", "PWD" };

    /// <summary>
    /// Sets the API Credentials
    /// </summary>
    /// <param name="Userid"></param>
    /// <param name="Pwd"></param>
    /// <param name="Signature"></param>
    /// <returns></returns>
    public void SetCredentials(string Userid, string Pwd, string Signature) {
      APIUsername = Userid;
      APIPassword = Pwd;
      APISignature = Signature;
    }
    /// <summary>
    /// Sets the Return/Cancel URLs
    /// </summary>
    /// <param name="Return"></param>
    /// <param name="Cancel"></param>
    /// <returns></returns>
    public void SetURLs(string Return, string Cancel) {
      returnURL = Return;
      cancelURL = Cancel;
    }
    /// <summary>
    /// ShortcutExpressCheckout: The method that calls SetExpressCheckout API
    /// </summary>
    /// <param name="amt"></param>
    /// <param ref name="token"></param>
    /// <param ref name="retMsg"></param>
    /// <returns></returns>
    public bool SetExpressCheckout(string amt, string description, string localeId, ref string token, ref string retMsg) {
      var url = pendpointurl;
      var host = "www.paypal.com";
      if (_bSandbox) {
        url = sandboxapiurl;
        host = "www.sandbox.paypal.com";
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME-Sandbox"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD-Sandbox"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE-Sandbox"];
      } else {
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE"];
      }

      var encoder = new NVPCodec();
      encoder["METHOD"] = "SetExpressCheckout";
      encoder["USER"] = APIUsername;
      encoder["PWD"] = APIPassword;
      encoder["LOCALECODE"] = localeId;
      encoder["SIGNATURE"] = APISignature;
      encoder["RETURNURL"] = returnURL;
      encoder["CANCELURL"] = cancelURL;
      encoder["PAYMENTREQUEST_0_AMT"] = amt;
      encoder["PAYMENTREQUEST_0_ITEMAMT"] = amt;
      encoder["PAYMENTREQUEST_0_SHIPPINGAMT"] = "0.00";
      encoder["PAYMENTREQUEST_0_DESC"] = description;
      encoder["PAYMENTREQUEST_0_CURRENCYCODE"] = WebConfigurationManager.AppSettings["paypal:CURRENCYCODE"];
      encoder["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale";

      var pStrrequestforNvp = encoder.Encode();
      var pStresponsenvp = HttpCall(pStrrequestforNvp, url);

      var decoder = new NVPCodec();
      decoder.Decode(pStresponsenvp);

      var strAck = decoder["ACK"].ToLower();
      if ((strAck == "success" || strAck == "successwithwarning")) {
        token = decoder["TOKEN"];
        var ECURL = "https://" + host + "/cgi-bin/webscr?cmd=_express-checkout" + "&token=" + token;
        retMsg = ECURL;
        return true;
      }
      retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
               "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
               "Desc2=" + decoder["L_LONGMESSAGE0"];
      return false;
    }

    /// <summary>
    /// MarkExpressCheckout: The method that calls SetExpressCheckout API, invoked from the 
    /// Billing Page EC placement
    /// </summary>
    /// <param name="amt"></param>
    /// <param ref name="token"></param>
    /// <param ref name="retMsg"></param>
    /// <returns></returns>
    public bool SetExpressCheckout(string amt, string description,
                        string shipToName, string shipToStreet, string shipToStreet2,
                        string shipToCity, string shipToState, string shipToZip,
                        string shipToCountryCode, ref string token, ref string retMsg) {
      var host = "www.paypal.com";
      var url = pendpointurl;
      if (_bSandbox) {
        url = sandboxapiurl;
        host = "www.sandbox.paypal.com";
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME-Sandbox"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD-Sandbox"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE-Sandbox"];
      } else {
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE"];
      }

      var encoder = new NVPCodec();
      encoder["METHOD"] = "SetExpressCheckout";
      encoder["USER"] = APIUsername;
      encoder["PWD"] = APIPassword;
      encoder["SIGNATURE"] = APISignature;
      encoder["RETURNURL"] = returnURL;
      encoder["CANCELURL"] = cancelURL;
      encoder["PAYMENTREQUEST_0_AMT"] = amt;
      encoder["PAYMENTREQUEST_0_ITEMAMT"] = amt;
      encoder["PAYMENTREQUEST_0_SHIPPINGAMT"] = "0.00";
      encoder["PAYMENTREQUEST_0_DESC"] = description;
      encoder["PAYMENTREQUEST_0_CURRENCYCODE"] = WebConfigurationManager.AppSettings["paypal:CURRENCYCODE"];
      encoder["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale";

      //Optional Shipping Address entered on the merchant site
      encoder["SHIPTONAME"] = shipToName;
      encoder["SHIPTOSTREET"] = shipToStreet;
      encoder["SHIPTOSTREET2"] = shipToStreet2;
      encoder["SHIPTOCITY"] = shipToCity;
      encoder["SHIPTOSTATE"] = shipToState;
      encoder["SHIPTOZIP"] = shipToZip;
      encoder["SHIPTOCOUNTRYCODE"] = shipToCountryCode;

      var pStrrequestforNvp = encoder.Encode();
      var pStresponsenvp = HttpCall(pStrrequestforNvp, url);

      var decoder = new NVPCodec();
      decoder.Decode(pStresponsenvp);

      var strAck = decoder["ACK"].ToLower();
      if ((strAck == "success" || strAck == "successwithwarning")) {
        token = decoder["TOKEN"];
        var ECURL = "https://" + host + "/cgi-bin/webscr?cmd=_express-checkout" + "&token=" + token;
        retMsg = ECURL;
        return true;
      }
      retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
               "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
               "Desc2=" + decoder["L_LONGMESSAGE0"];
      return false;
    }


    /// <summary>
    /// GetShippingDetails: The method that calls SetExpressCheckout API, invoked from the 
    /// Billing Page EC placement
    /// </summary>
    /// <param name="token"></param>
    /// <param ref name="retMsg"></param>
    /// <returns></returns>
    public bool GetExpressCheckoutDetails(string token, ref string PayerId, ref NVPCodec shippingAddress, ref string retMsg) {
      var url = pendpointurl;
      if (_bSandbox) {
        url = sandboxapiurl;
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME-Sandbox"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD-Sandbox"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE-Sandbox"];
      } else {
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE"];
      }

      var encoder = new NVPCodec();
      encoder["METHOD"] = "GetExpressCheckoutDetails";
      encoder["TOKEN"] = token;
      encoder["USER"] = APIUsername;
      encoder["PWD"] = APIPassword;
      encoder["SIGNATURE"] = APISignature;

      var pStrrequestforNvp = encoder.Encode();
      var pStresponsenvp = HttpCall(pStrrequestforNvp, url);

      var decoder = new NVPCodec();
      decoder.Decode(pStresponsenvp);

      var strAck = decoder["ACK"].ToLower();
      if ((strAck == "success" || strAck == "successwithwarning")) {
        shippingAddress = decoder;
        PayerId = decoder["PAYERID"];
        return true;
      }
      retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
               "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
               "Desc2=" + decoder["L_LONGMESSAGE0"];
      return false;
    }

    /// <summary>
    /// ConfirmPayment: The method that calls SetExpressCheckout API, invoked from the 
    /// Billing Page EC placement
    /// </summary>
    /// <param name="token"></param>
    /// <param ref name="retMsg"></param>
    /// <returns></returns>
    public bool DoExpressCheckoutPayment(string amt, string description, string token, string PayerId, ref NVPCodec decoder, ref string retMsg) {
      var url = pendpointurl;
      if (_bSandbox) {
        url = sandboxapiurl;
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME-Sandbox"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD-Sandbox"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE-Sandbox"];
      } else {
        APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME"];
        APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD"];
        APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE"];
      }

      var encoder = new NVPCodec();
      encoder["METHOD"] = "DoExpressCheckoutPayment";
      encoder["TOKEN"] = token;
      encoder["USER"] = APIUsername;
      encoder["PWD"] = APIPassword;
      encoder["SIGNATURE"] = APISignature;      
      encoder["PAYERID"] = PayerId;
      encoder["PAYMENTREQUEST_0_AMT"] = amt;
      encoder["PAYMENTREQUEST_0_ITEMAMT"] = amt;
      encoder["PAYMENTREQUEST_0_SHIPPINGAMT"] = "0.00";
      encoder["PAYMENTREQUEST_0_DESC"] = description;
      encoder["PAYMENTREQUEST_0_CURRENCYCODE"] = WebConfigurationManager.AppSettings["paypal:CURRENCYCODE"];
      encoder["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale";

      var pStrrequestforNvp = encoder.Encode();
      var pStresponsenvp = HttpCall(pStrrequestforNvp, url);

      decoder = new NVPCodec();
      decoder.Decode(pStresponsenvp);

      var strAck = decoder["ACK"].ToLower();
      if (strAck != null && (strAck == "success" || strAck == "successwithwarning")) {
        return true;
      }
      retMsg = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
               "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
               "Desc2=" + decoder["L_LONGMESSAGE0"];
      return false;
    }

    /// <summary>
    /// HttpCall: The main method that is used for all API calls
    /// </summary>
    /// <param name="NvpRequest"></param>
    /// <returns></returns>
    public string HttpCall(string NvpRequest, string url) //CallNvpServer
    {      

      //To Add the credentials from the profile
      var strPost = NvpRequest + "&" + buildCredentialsNVPString();
      strPost = strPost + "&BUTTONSOURCE=" + HttpUtility.UrlEncode(BNCode);

      var objRequest = (HttpWebRequest)WebRequest.Create(url);
      objRequest.Timeout = Timeout;
      objRequest.Method = "POST";
      objRequest.ContentLength = strPost.Length;

      try {
        using (var myWriter = new StreamWriter(objRequest.GetRequestStream())) {
          myWriter.Write(strPost);
        }
      } catch (Exception e) {
        Trace.TraceError(e.Message);
        /*
        if (log.IsFatalEnabled)
        {
            log.Fatal(e.Message, this);
        }*/
      }

      //Retrieve the Response returned from the NVP API call to PayPal
      var objResponse = (HttpWebResponse)objRequest.GetResponse();
      string result;
      using (var sr = new StreamReader(objResponse.GetResponseStream())) {
        result = sr.ReadToEnd();
      }

      //Logging the response of the transaction
      /* if (log.IsInfoEnabled)
       {
           log.Info("Result :" +
                     " Elapsed Time : " + (DateTime.Now - startDate).Milliseconds + " ms" +
                    result);
       }
       */
      return result;
    }

    /// <summary>
    /// Credentials added to the NVP string
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    private string buildCredentialsNVPString() {
      var codec = new NVPCodec();

      if (!IsEmpty(APIUsername))
        codec["USER"] = APIUsername;

      if (!IsEmpty(APIPassword))
        codec["PWD"] = APIPassword;

      if (!IsEmpty(APISignature))
        codec["SIGNATURE"] = APISignature;

      if (!IsEmpty(Subject))
        codec["SUBJECT"] = Subject;

      codec["VERSION"] = "63.0";

      return codec.Encode();
    }

    /// <summary>
    /// Returns if a string is empty or null
    /// </summary>
    /// <param name="s">the string</param>
    /// <returns>true if the string is not null and is not empty or just whitespace</returns>
    public static bool IsEmpty(string s) {
      return s == null || s.Trim() == string.Empty;
    }
  }

  public sealed class NVPCodec : NameValueCollection {
    private const string AMPERSAND = "&";
    private const string EQUALS = "=";
    private static readonly char[] AMPERSAND_CHAR_ARRAY = AMPERSAND.ToCharArray();
    private static readonly char[] EQUALS_CHAR_ARRAY = EQUALS.ToCharArray();

    /// <summary>
    /// Returns the built NVP string of all name/value pairs in the Hashtable
    /// </summary>
    /// <returns></returns>
    public string Encode() {
      var sb = new StringBuilder();
      var firstPair = true;
      foreach (var kv in AllKeys) {
        var name = HttpUtility.UrlEncode(kv);
        var value = HttpUtility.UrlEncode(this[kv]);
        if (!firstPair) {
          sb.Append(AMPERSAND);
        }
        sb.Append(name).Append(EQUALS).Append(value);
        firstPair = false;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Decoding the string
    /// </summary>
    /// <param name="nvpstring"></param>
    public void Decode(string nvpstring) {
      Clear();
      foreach (var nvp in nvpstring.Split(AMPERSAND_CHAR_ARRAY)) {
        var tokens = nvp.Split(EQUALS_CHAR_ARRAY);
        if (tokens.Length >= 2) {
          var name = HttpUtility.UrlDecode(tokens[0]);
          var value = HttpUtility.UrlDecode(tokens[1]);
          Add(name, value);
        }
      }
    }


    #region Array methods
    public void Add(string name, string value, int index) {
      this.Add(GetArrayName(index, name), value);
    }

    public void Remove(string arrayName, int index) {
      this.Remove(GetArrayName(index, arrayName));
    }

    /// <summary>
    /// 
    /// </summary>
    public string this[string name, int index] {
      get {
        return this[GetArrayName(index, name)];
      }
      set {
        this[GetArrayName(index, name)] = value;
      }
    }

    private static string GetArrayName(int index, string name) {
      if (index < 0) {
        throw new ArgumentOutOfRangeException("index", "index can not be negative : " + index);
      }
      return name + index;
    }
    #endregion
  }
}

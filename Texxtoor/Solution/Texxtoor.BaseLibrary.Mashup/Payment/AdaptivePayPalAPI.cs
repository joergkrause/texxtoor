using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Texxtoor.BaseLibrary.Mashup.Payment.Adaptive {
  // <summary>
  /// Summary description for NVPAPICaller
  /// </summary>
  /// // <summary>
  /// Summary description for NVPAPICaller
  /// </summary>
  /// 
  public class AdaptivePayPalAPI {
    //private static readonly ILog log = LogManager.GetLogger(typeof(NVPAPICaller));

    private string pendpointurl = "https://svcs.paypal.com/AdaptivePayments/Pay";
    private const string CVV2 = "CVV2";

    //Flag that determines the PayPal environment (live or sandbox)
    private readonly bool _bSandbox = Boolean.Parse(WebConfigurationManager.AppSettings["paypal:UseSandBox"]);

    private const string SIGNATURE = "SIGNATURE";
    private const string PWD = "PWD";
    private const string ACCT = "ACCT";

    //Replace <API_USERNAME> with your API Username
    //Replace <API_PASSWORD> with your API Password
    //Replace <API_SIGNATURE> with your Signature
    public string APIUsername = WebConfigurationManager.AppSettings["paypal:API_USERNAME"].ToString();
    private string APIPassword = WebConfigurationManager.AppSettings["paypal:API_PASSWORD"].ToString();
    private string APISignature = WebConfigurationManager.AppSettings["paypal:API_SIGNATURE"].ToString();
    private string APIApplicationId = WebConfigurationManager.AppSettings["paypal:API_ApplicationId"].ToString();
    private string APIRequestDataFormat = WebConfigurationManager.AppSettings["paypal:API_RequestDataFormat"].ToString();
    private string APIResponseDataFormat = WebConfigurationManager.AppSettings["paypal:API_ResponseDataFormat"].ToString();

    //private string APISignature = WebConfigurationManager.AppSettings["APISignature"].ToString();


    //HttpWebRequest Timeout specified in milliseconds 
    private const int Timeout = 35000;
    private static readonly string[] SECURED_NVPS = new string[] { ACCT, CVV2, SIGNATURE, PWD };

    /// <summary>
    /// Sets the API Credentials (Set the Credential Explicitly).
    /// </summary>
    /// <param name="Userid"></param>
    /// <param name="Pwd"></param>
    /// <param name="Signature"></param>
    /// <param name="ApplicationId"></param>
    /// <param name="ReqDataFormat"></param>
    /// <param name="ResDataFormat"></param>
    /// <returns></returns>
    public void SetCredentials(string Userid, string Pwd, string Signature, string ApplicationId, string ReqDataFormat, string ResDataFormat) {
      APIUsername = Userid;
      APIPassword = Pwd;
      APISignature = Signature;
      APIApplicationId = ApplicationId;
      APIRequestDataFormat = ReqDataFormat;
      APIResponseDataFormat = ResDataFormat;
    }

    /// <summary>
    /// AdaptivePay: The method that calls Adaptive Pay API, 
    /// </summary>
    /// <param></param>
    /// <param></param>
    /// <param name="senderPayPalId"></param>
    /// <param name="receiverPayPalId"></param>
    /// <param name="widthrawalAmount"></param>
    /// <param name="cancelURL"></param>
    /// <param name="retMsg"></param>
    /// <param name="returnURL"></param>
    /// <returns></returns>
    public bool AdaptivePay(string senderPayPalId, string receiverPayPalId, string widthrawalAmount, string returnURL, string cancelURL, ref string retMsg) {

      if (_bSandbox) {
        pendpointurl = "https://svcs.sandbox.paypal.com/AdaptivePayments/Pay";
      }

      var host = HttpContext.Current.Request.UserHostName;

      var encoder = new AdaptiveNVPCodec();
      encoder["actionType"] = "PAY";
      encoder["cancelUrl"] = cancelURL;
      encoder["currencyCode"] = WebConfigurationManager.AppSettings["paypal:CURRENCYCODE"].ToString();
      encoder["senderEmail"] = senderPayPalId;
      encoder["receiverList.receiver(0).email"] = receiverPayPalId;
      encoder["receiverList.receiver(0).amount"] = widthrawalAmount;
      encoder["requestEnvelope.errorLanguage"] = "en_US";
      encoder["requestEnvelope.detailLevel"] = "ReturnAll";
      encoder["returnUrl"] = returnURL;

      string pStrrequestforNvp = encoder.Encode();
      string pStresponsenvp = HttpCall(pStrrequestforNvp);

      var decoder = new AdaptiveNVPCodec();
      decoder.Decode(pStresponsenvp);

      string strAck = decoder["responseEnvelope.ack"].ToLower();
      if ((strAck == "success" || strAck == "successwithwarning")) {
        return true;
      } else {
        retMsg = "ErrorCode=" + decoder["error(0).errorId"] + "&" +
            "Desc=" + decoder["error(0).message"] + "&" +
            "Desc2=" + decoder["error(0).parameter(0)"];

        return false;
      }
    }

    /// <summary>
    /// HttpCall: The main method that is used for all API calls
    /// </summary>
    /// <param name="NvpRequest"></param>
    /// <returns></returns>
    public string HttpCall(string NvpRequest) //CallNvpServer
    {
      string url = pendpointurl;

      //assign to Post string
      string strPost = NvpRequest;


      HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
      objRequest.Timeout = Timeout;
      objRequest.Method = "POST";
      objRequest.ContentLength = strPost.Length;

      //Set Request Headers
      Hashtable NVPHeaders = buildRequestNVPHeaders();
      foreach (DictionaryEntry de in NVPHeaders)
        objRequest.Headers.Add(de.Key.ToString(), de.Value.ToString());

      try {
        using (StreamWriter myWriter = new StreamWriter(objRequest.GetRequestStream())) {
          myWriter.Write(strPost);
        }
      } catch (Exception e) {
        /*
        if (log.IsFatalEnabled)
        {
            log.Fatal(e.Message, this);
        }*/
      }

      //Retrieve the Response returned from the NVP API call to PayPal


      HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
      string result;
      using (StreamReader sr = new StreamReader(objResponse.GetResponseStream())) {
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
    /// Credentials added to the NVP Headers
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    private Hashtable buildRequestNVPHeaders() {
      Hashtable NVPHeaders = new Hashtable();
      NVPHeaders["X-PAYPAL-SECURITY-USERID"] = APIUsername;
      NVPHeaders["X-PAYPAL-SECURITY-PASSWORD"] = APIPassword;
      NVPHeaders["X-PAYPAL-SECURITY-SIGNATURE"] = APISignature;
      NVPHeaders["X-PAYPAL-APPLICATION-ID"] = APIApplicationId;
      NVPHeaders["X-PAYPAL-REQUEST-DATA-FORMAT"] = APIRequestDataFormat;
      NVPHeaders["X-PAYPAL-RESPONSE-DATA-FORMAT"] = APIResponseDataFormat;
      return NVPHeaders;
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

  public sealed class AdaptiveNVPCodec : NameValueCollection {
    private const string AMPERSAND = "&";
    private const string EQUALS = "=";
    private static readonly char[] AMPERSAND_CHAR_ARRAY = AMPERSAND.ToCharArray();
    private static readonly char[] EQUALS_CHAR_ARRAY = EQUALS.ToCharArray();

    /// <summary>
    /// Returns the built NVP string of all name/value pairs in the Hashtable
    /// </summary>
    /// <returns></returns>
    public string Encode() {
      StringBuilder sb = new StringBuilder();
      bool firstPair = true;
      foreach (string kv in AllKeys) {
        string name = HttpUtility.UrlEncode(kv);
        string value = HttpUtility.UrlEncode(this[kv]);
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
      foreach (string nvp in nvpstring.Split(AMPERSAND_CHAR_ARRAY)) {
        string[] tokens = nvp.Split(EQUALS_CHAR_ARRAY);
        if (tokens.Length >= 2) {
          string name = HttpUtility.UrlDecode(tokens[0]);
          string value = HttpUtility.UrlDecode(tokens[1]);
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

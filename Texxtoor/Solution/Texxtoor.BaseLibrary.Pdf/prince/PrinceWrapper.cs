using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.BaseLibrary.Pdf.Prince {

  public interface IPrince {
    void SetEncryptInfo(int keyBits, string userPassword, string ownerPassword, bool disallowPrint,
                        bool disallowModify, bool disallowCopy, bool disallowAnnotate);
    void AddStyleSheet(string cssPath);
    void ClearStyleSheets();
    void AddScript(string jsPath);
    void ClearScripts();
    void SetLicenseFile(string file);
    void SetLicenseKey(string key);
    void SetHtml(bool html);
    void SetJavaScript(bool js);
    void SetHttpUser(string user);
    void SetHttpPassword(string password);
    void SetHttpProxy(string proxy);
    void SetInsecure(bool insecure);
    void SetLog(string logFile);
    void SetBaseUrl(string baseurl);
    void SetFileRoot(string fileroot);
    void SetXInclude(bool xinclude);
    void SetEmbedFonts(bool embed);
    void SetSubsetFonts(bool embedSubset);
    void SetCompress(bool compress);
    void SetEncrypt(bool encrypt);
    bool Convert(string xmlPath);
    bool Convert(string xmlPath, string pdfPath);
    bool Convert(string xmlPath, Stream pdfOutput);
    bool Convert(Stream xmlInput, string pdfPath);
    bool Convert(Stream xmlInput, Stream pdfOutput);
    bool ConvertMemoryStream(MemoryStream xmlInput, Stream pdfOutput);
    bool ConvertString(string xmlInput, Stream pdfOutput);
    bool ConvertMultiple(string[] xmlPaths, string pdfPath);

  }

  public class PrinceFilter : System.IO.Stream {

    private readonly IPrince _prince;
    private readonly Stream _oldFilter;
    private readonly MemoryStream _memStream;

    public PrinceFilter(IPrince prince, Stream oldFilter) {
      _prince = prince;
      _oldFilter = oldFilter;
      _memStream = new MemoryStream();
    }

    public override bool CanSeek {
      get { return false; }
    }

    public override bool CanWrite {
      get { return true; }
    }

    public override bool CanRead {
      get { return false; }
    }

    public override long Position {
      get { return 0; }
      set { }
    }

    public override long Length {
      get { return 0; }
    }

    public override int Read(byte[] buffer, int offset, int count) {
      return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) {
      return 0;
    }

    public override void SetLength(long value) {
      // do nothing
    }

    public override void Write(byte[] buffer, int offset, int count) {
      _memStream.Write(buffer, offset, count);
    }

    public override void Flush() {
      // FIXME?
    }

    public override void Close() {
      _prince.ConvertMemoryStream(_memStream, _oldFilter);
      _oldFilter.Close();
    }

  }

  public class Prince : IPrince {

    private readonly string _mPrincePath;
    private string mStyleSheets;
    private string mJavaScripts;
    private string mLicenseFile;
    private string mLicenseKey;
    private bool mHTML;
    private bool mJavaScript;
    private string mHttpUser;
    private string mHttpPassword;
    private string mHttpProxy;
    private bool mInsecure;
    private string mLog;
    private string mBaseURL;
    private string mFileRoot;
    private bool mXInclude;
    private bool mEmbedFonts;
    private bool mSubsetFonts;
    private bool mCompress;
    private bool mEncrypt;
    private string mEncryptInfo;

    public Prince() {
      _mPrincePath = "";
      mStyleSheets = "";
      mJavaScripts = "";
      mLicenseFile = "";
      mLicenseKey = "";
      mHTML = false;
      mJavaScript = false;
      mHttpUser = "";
      mHttpPassword = "";
      mHttpProxy = "";
      mInsecure = false;
      mLog = "";
      mBaseURL = "";
      mFileRoot = "";
      mXInclude = true;
      mEmbedFonts = true;
      mSubsetFonts = true;
      mCompress = true;
      mEncrypt = false;
      mEncryptInfo = "";
    }

    public Prince(string princePath) {
      _mPrincePath = princePath;
      mStyleSheets = "";
      mJavaScripts = "";
      mHTML = false;
      mJavaScript = false;
      mHttpUser = "";
      mHttpPassword = "";
      mHttpProxy = "";
      mInsecure = false;
      mLog = "";
      mBaseURL = "";
      mFileRoot = "";
      mXInclude = true;
      mEmbedFonts = true;
      mSubsetFonts = true;
      mCompress = true;
      mEncrypt = false;
      mEncryptInfo = "";
    }

    public void SetLicenseFile(string file) {
      mLicenseFile = file;
    }

    public void SetLicenseKey(string key) {
      mLicenseKey = key;
    }

    public void SetHtml(bool html) {
      mHTML = html;
    }

    public void SetJavaScript(bool js) {
      mJavaScript = js;
    }

    public void SetHttpUser(string user) {
      mHttpUser = user;
    }

    public void SetHttpPassword(string password) {
      mHttpPassword = password;
    }

    public void SetHttpProxy(string proxy) {
      mHttpProxy = proxy;
    }

    public void SetInsecure(bool insecure) {
      mInsecure = insecure;
    }

    public void SetLog(string logFile) {
      mLog = logFile;
    }

    public void SetBaseUrl(string baseurl) {
      mBaseURL = baseurl;
    }

    public void SetFileRoot(string fileroot) {
      mFileRoot = fileroot;
    }

    public void SetXInclude(bool xinclude) {
      mXInclude = xinclude;
    }

    public void SetEmbedFonts(bool embed) {
      mEmbedFonts = embed;
    }

    public void SetSubsetFonts(bool subset) {
      mSubsetFonts = subset;
    }

    public void SetCompress(bool compress) {
      mCompress = compress;
    }

    public void SetEncrypt(bool encrypt) {
      mEncrypt = encrypt;
    }

    public void SetEncryptInfo(int keyBits,
                               string userPassword,
                               string ownerPassword,
                               bool disallowPrint,
                               bool disallowModify,
                               bool disallowCopy,
                               bool disallowAnnotate) {

      mEncrypt = true;

      if ((keyBits != 40) && (keyBits != 128)) {
        mEncryptInfo = "";
        throw new ApplicationException("Invalid value for keyBits: must be 40 or 128");
      }
      mEncryptInfo = "--encrypt "
                     + " --key-bits " + keyBits
                     + " --user-password=" + @"""" + CmdlineArgEscape2(CmdlineArgEscape1(userPassword)) + @""""
                     + " --owner-password=" + @"""" + CmdlineArgEscape2(CmdlineArgEscape1(ownerPassword)) + @"""" + " ";

      if (disallowPrint) {
        mEncryptInfo = mEncryptInfo + "--disallow-print ";
      }

      if (disallowModify) {
        mEncryptInfo = mEncryptInfo + "--disallow-modify ";
      }

      if (disallowCopy) {
        mEncryptInfo = mEncryptInfo + "--disallow-copy ";
      }

      if (disallowAnnotate) {
        mEncryptInfo = mEncryptInfo + "--disallow-annotate ";
      }
    }

    public void AddStyleSheet(string cssPath) {
      mStyleSheets = mStyleSheets + "-s " + @"""" + cssPath + @"""" + " ";
    }

    public void ClearStyleSheets() {
      mStyleSheets = "";
    }

    public void AddScript(string jsPath) {
      mJavaScripts = mJavaScripts + "--script " + @"""" + jsPath + @"""" + " ";
    }

    public void ClearScripts() {
      mJavaScripts = "";
    }

    private string GetArgs() {
      string args = "--server " + mStyleSheets + mJavaScripts;

      if (mEncrypt) {
        args = args + mEncryptInfo;
      }

      if (mHTML) {
        args = args + "-i html ";
      }

      if (mJavaScript) {
        args = args + "--javascript ";
      }

      if (mHttpUser != "") {
        args = args + @"--http-user=""" + CmdlineArgEscape2(CmdlineArgEscape1(mHttpUser)) + @""" ";
      }

      if (mHttpPassword != "") {
        args = args + @"--http-password=""" + CmdlineArgEscape2(CmdlineArgEscape1(mHttpPassword)) + @""" ";
      }

      if (mHttpProxy != "") {
        args = args + @"--http-proxy=""" + mHttpProxy + @""" ";
      }

      if (mInsecure) {
        args = args + "--ssl-blindly-trust-server ";
      }

      if (mLog != "") {
        args = args + @"--log=""" + mLog + @""" ";
      }

      if (mBaseURL != "") {
        args = args + @"--baseurl=""" + mBaseURL + @""" ";
      }

      if (mFileRoot != "") {
        args = args + @"--fileroot=""" + mFileRoot + @""" ";
      }

      if (mLicenseFile != "") {
        args = args + @"--license-file=""" + mLicenseFile + @""" ";
      }

      if (mLicenseKey != "") {
        args = args + @"--license-key=""" + mLicenseKey + @""" ";
      }

      if (!mXInclude) {
        args = args + "--no-xinclude ";
      }

      if (!mEmbedFonts) {
        args = args + "--no-embed-fonts ";
      }

      if (!mSubsetFonts) {
        args = args + "--no-subset-fonts ";
      }

      if (!mCompress) {
        args = args + "--no-compress ";
      }

      return args;
    }

    public bool Convert(string xmlPath) {
      var args = GetArgs() + '"' + xmlPath + '"';
      return Convert1(args);
    }

    public bool Convert(string xmlPath, string pdfPath) {
      var args = GetArgs() + '"' + xmlPath + '"' + " -o " + '"' + pdfPath + '"';
      return Convert1(args);
    }

    public bool ConvertMultiple(string[] xmlPaths, string pdfPath) {
      var docPaths = xmlPaths.Aggregate("", (current, doc) => current + '"' + doc + '"' + " ");
      var args = GetArgs() + docPaths + " -o " + '"' + pdfPath + '"';
      return Convert1(args);
    }

    public bool Convert(string xmlPath, Stream pdfOutput) {

      var buf = new byte[4096];

      if (!pdfOutput.CanWrite) {
        throw new ApplicationException("The pdfOutput stream is not writable");
      }
      var args = GetArgs() + "--silent " + @"""" + xmlPath + @""" -o -";
      var prs = StartPrince(args);

      prs.StandardInput.Close();

      var bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      while (bytesRead != 0) {
        pdfOutput.Write(buf, 0, bytesRead);
        bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      }
      prs.StandardOutput.Close();

      return ReadMessages(prs) == "success";
    }

    public bool Convert(Stream xmlInput, string pdfPath) {

      var buf = new byte[4096];
      var prs = new Process();

      if (!xmlInput.CanRead) {
        throw new ApplicationException("The xmlInput stream is not readable");
      }
      string args = GetArgs() + @"--silent - -o """ + pdfPath + @"""";
      prs = StartPrince(args);

      int bytesRead = xmlInput.Read(buf, 0, 4096);
      while (bytesRead != 0) {
        prs.StandardInput.BaseStream.Write(buf, 0, bytesRead);
        bytesRead = xmlInput.Read(buf, 0, 4096);
      }
      prs.StandardInput.Close();

      prs.StandardOutput.Close();

      return ReadMessages(prs) == "success";
    }

    public bool Convert(Stream xmlInput, Stream pdfOutput) {

      var buf = new byte[4096];
      string args;

      if (!xmlInput.CanRead) {
        throw new ApplicationException("The xmlInput stream is not readable");
      }
      if (!pdfOutput.CanWrite) {
        throw new ApplicationException("The pdfOutput stream is not writable");
      }
      args = GetArgs() + "--silent -";
      var prs = StartPrince(args);

      var bytesRead = xmlInput.Read(buf, 0, 4096);
      while (bytesRead != 0) {
        prs.StandardInput.BaseStream.Write(buf, 0, bytesRead);
        bytesRead = xmlInput.Read(buf, 0, 4096);
      }
      prs.StandardInput.Close();

      bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      while (bytesRead != 0) {
        pdfOutput.Write(buf, 0, bytesRead);
        bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      }
      prs.StandardOutput.Close();

      return ReadMessages(prs) == "success";
    }

    public bool ConvertMemoryStream(MemoryStream xmlInput, Stream pdfOutput) {

      var buf = new byte[4096];
      string args;

      if (!pdfOutput.CanWrite) {
        throw new ApplicationException("The pdfOutput stream is not writable");
      }
      args = GetArgs() + "--silent -";
      var prs = StartPrince(args);

      xmlInput.WriteTo(prs.StandardInput.BaseStream);
      prs.StandardInput.Close();

      int bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      while (bytesRead != 0) {
        pdfOutput.Write(buf, 0, bytesRead);
        bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      }
      prs.StandardOutput.Close();

      return ReadMessages(prs) == "success";
    }

    public bool ConvertString(string xmlInput, Stream pdfOutput) {

      var buf = new byte[4096];

      if (!pdfOutput.CanWrite) {
        throw new ApplicationException("The pdfOutput stream is not writable");
      }
      var args = GetArgs() + "--silent -";
      var prs = StartPrince(args);

      byte[] stringBytes = Encoding.UTF8.GetBytes(xmlInput);
      prs.StandardInput.BaseStream.Write(stringBytes, 0, stringBytes.Length);
      prs.StandardInput.Close();

      int bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      while (bytesRead != 0) {
        pdfOutput.Write(buf, 0, bytesRead);
        bytesRead = prs.StandardOutput.BaseStream.Read(buf, 0, 4096);
      }
      prs.StandardOutput.Close();

      return ReadMessages(prs) == "success";
    }

    private bool Convert1(string args) {
      var pr = StartPrince(args);

      if (pr != null) {
        return ReadMessages(pr) == "success";
      }
      return false;
    }

    private Process StartPrince(string args) {
      const int ERROR_FILE_NOT_FOUND = 2;
      const int ERROR_PATH_NOT_FOUND = 3;
      const int ERROR_ACCESS_DENIED = 5;

      var pr = new Process();

      pr.StartInfo.FileName = _mPrincePath;
      pr.StartInfo.Arguments = args;
      pr.StartInfo.UseShellExecute = false;
      pr.StartInfo.CreateNoWindow = true;
      pr.StartInfo.RedirectStandardInput = true;
      pr.StartInfo.RedirectStandardOutput = true;
      pr.StartInfo.RedirectStandardError = true;

      try {
        pr.Start();

        if (!pr.HasExited) {
          return pr;
        }
        throw new ApplicationException("Error starting Prince: " + _mPrincePath);
      } catch (System.ComponentModel.Win32Exception ex) {
        string msg;
        msg = ex.Message;
        if (ex.NativeErrorCode == ERROR_FILE_NOT_FOUND) {
          msg = msg + " -- Please verify that Prince.exe is in the directory";
        } else if (ex.NativeErrorCode == ERROR_ACCESS_DENIED) {
          msg = msg + " -- Please check system permission to run Prince.";
        } else if (ex.NativeErrorCode == ERROR_PATH_NOT_FOUND) {
          msg = msg + " -- Please check Prince path.";
        }
        // just use ex.this.sage
        throw new ApplicationException(msg);
      }
    }

    private static string ReadMessages(Process prs) {
      var stdErrFromPr = prs.StandardError;

      var result = "";
      var line = stdErrFromPr.ReadLine();
      while (line != null) {
        if (line.Substring(0, 4) == "fin|") {
          result = line.Substring(4, (line.Length - 4));
        }
        line = stdErrFromPr.ReadLine();
      }
      stdErrFromPr.Close();
      return result;
    }

    private static string CmdlineArgEscape1(string arg) {
      int pos;

      if (arg.Length == 0) {
        //return empty  string 
        return arg;
      }
      //chr(34) is character double quote (" ), chr(92) is character backslash ( \ )
      for (pos = (arg.Length - 1); pos > 0; pos--) {
        if (arg[pos] != '"') continue;
        // if there is a double quote in the arg string 
        // find number of backslashes preceding the double quote (" )
        int numSlashes = 0;
        while ((pos - 1 - numSlashes) >= 0) {
          if (arg[pos - 1 - numSlashes] == '\\') {
            numSlashes += 1;
          } else {
            break;
          }
        }

        var rightSubstring = arg.Substring(pos + 1);
        var leftSubstring = arg.Substring(0, (pos - numSlashes));

        var middleSubstring = "\\";
        for (var i = 1; i <= numSlashes; i++) {
          middleSubstring = middleSubstring + '\\' + '\\';
        }

        middleSubstring = middleSubstring + '"';

        return CmdlineArgEscape1(leftSubstring) + middleSubstring + rightSubstring;
      }

      //no double quote  found, return string itself
      return arg;
    }

    private static string CmdlineArgEscape2(string arg) {
      int pos;

      var numEndingSlashes = 0;
      for (pos = (arg.Length - 1); pos > 0; pos--) {
        if (arg[pos] == '\\') {
          numEndingSlashes += 1;
        } else {
          break;
        }
      }

      for (var i = 1; i < numEndingSlashes; i++) {
        arg = arg + '\\';
      }

      return arg;

    }
  }

}
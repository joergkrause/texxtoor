using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.Editor.Core
{
    class PDFConstants : Manager<PDFConstants> 
    {
        
        public string HTMLCoverImageTag = " <div style=\"text-align:center\"><img alt=\"cover\" style=\"height:890px; width:685px;\" src=\"{0}\" /></div>";
        public string HTMLTableStartTag = "<table class=\"styleTOC\">";
        public string RegularExpressionHeader = @"<(?<close>/)?h(?<header>\d)>(?<text>(.+?))\s*</h\d>";
        public string HTMLTableEndTag = "</table>";
        public string HTMLPageBreakTag = "<P CLASS=\"breakhere\">";
        public string HTMLCreateIndexTag = "<createindex>  </createindex>";
     
        public string HTMLTOCH1Tag = "<tr><td colspan=\"3\"><a id=\"TOCEntry_{0}_ID\" class=\"h1\" href=\"#{0}\">{1}</a></td></tr>";
        public string HTMLTOCH2Tag =  "<tr><td style=\"width: 10%;\"></td><td colspan=\"2\"><a id=\"TOCEntry_{0}_ID\" class=\"h2\" href=\"#{0}\">{1}</a></td></tr>";
        public string HTMLTOCH3Tag = "<tr><td></td><td style=\"width: 10%;\"></td><td><a id=\"TOCEntry_{0}_ID\" class=\"h3\" href=\"#{0}\">{1}</a></td></tr>";
        public string HTMLReplaceTOCH1Tag = "<h1><a id=\"TOCEntry_{0}_Target_ID\" style=\"font-size:24px\" class=\"bookmark\" name=\"{0}\"><span class=\"h1\" title=\"{1}\">{1}</span></a></h1>";
        public string HTMLReplaceTOCH2Tag = "<h2><a id=\"TOCEntry_{0}_Target_ID\" style=\"font-size:20px\" class=\"bookmark\" name=\"{0}\"><span class=\"h2\" title=\"{1}\">{1}</span></a></h2>";
        public string HTMLReplaceTOCH3Tag = "<h3><a id=\"TOCEntry_{0}_Target_ID\" style=\"font-size:16px\" class=\"bookmark\" name=\"{0}\"><span class=\"h3\" title=\"{1}\">{1}</span></a></h3>";
        public string HTMLBookmarkTag = "A[class=\"bookmark\"]";
        public string TOCENTRYID = "#TOCEntry_{0}_ID";
        public string TOCTARGETID = "#TOCEntry_{0}_Target_ID";
        public string HTMLMETAContentTag="<P style=\"page-break-before: always; \"><br><P style=\"page-break-before: always; \">" +
                                    "Title : ASP.NET Extensibility<br>Author : Jorge krause<br>Published by: Jorge Krause<br>Published Date: 25-09-2012<br>" +
                                    "<P style=\"page-break-before: always; \"><br><P style=\"page-break-before: always; \">";
        public string RegularExpressionIndex = "<span [^>]*class=(\"|')(?<classid>.+?)(\"|')*title=(\"|')(?<titlename>.+?)(\"|')>(?<content>.*?)</span>";
        public string RegularExpressionCreateIndex = "<createindex>(.*)</createindex>";
        public string HTMLTROpenCloseTag = "<tr><td>{0}</td></tr>";
        public string HTMLIndexContent = "<tr><td><a id=\"TOCIndex_{0}_ID\">{1}</a></td></tr>";
        public string HTMLSPANTagforH1="<span style=\"font-size:24px\"  title=\"{0}\">{1}</span>" ;
        public string HTMLSPANTagforH2 = "<span style=\"font-size:20px\"  title=\"{0}\">{1}</span>";
        public string HTMLSPANTagforH3 = "<span style=\"font-size:16px\"  title=\"{0}\">{1}</span>";
        public string HTMLSPANTagforH1TargetId = "<span id=\"TOCIndex_{0}_Target_ID\" style=\"font-size:24px\"  title=\"{1}\">{2}</span>";
        public string HTMLSPANTagforH2TargetId = "<span id=\"TOCIndex_{0}_Target_ID\" style=\"font-size:20px\"  title=\"{1}\">{2}</span>";
        public string HTMLSPANTagforH3TargetId = "<span id=\"TOCIndex_{0}_Target_ID\" style=\"font-size:16px\"  title=\"{1}\">{2}</span>";
     }
}

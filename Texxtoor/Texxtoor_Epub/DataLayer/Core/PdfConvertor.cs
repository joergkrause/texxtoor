using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using EvoPdf.HtmlToPdf;
using EvoPdf.HtmlToPdf.PdfDocument;

namespace Texxtoor.Editor.Core {


     public class PdfConvertor : Manager<PdfConvertor> {
    
        public bool Embedfonts
        {
            get;
            set;
        }
        public int HTMLViewerHeight { get; set; }
        public int HtmlViewerWidth { get; set; }
        public bool JavascriptEnabled { get; set; }
        public string HtmlURL { get; set; }
        
         #region Variables to generate TOC
        private int countTOC = 0;
        private const int PAGE_NUMBER_FONT_SIZE = 10;
        public static string TOC = PDFConstants.Instance.HTMLTableStartTag;

        #endregion

        #region Variables to generate Index
        List<string> lstTemp = new List<string>();
        List<string> Old_lstTemp = new List<string>();
        int countIndex = 0;
        string Index = PDFConstants.Instance.HTMLTableStartTag;
        List<int> NewIndexList = new List<int>();
        public static string PDFPath=string.Empty;
        #endregion

        private ImgConverter GetImgConverter()
        {
            ImgConverter imgConverter = new ImgConverter();

            // set the HTML viewer width and height in pixels
            // the default value is 1024 pixels for width and 0 for height 
            imgConverter.HtmlViewerWidth = HtmlViewerWidth;
            imgConverter.HtmlViewerHeight = HTMLViewerHeight;
            // set if the JavaScript is executed during conversion - default value is true
            imgConverter.JavaScriptEnabled = JavascriptEnabled;

            return imgConverter;
        }
        public void GeneratePDF(string HtmlURL,string CoverImagePath)
        {
               
                #region Modify HTML for TOC
                
                WebClient client = new WebClient();
                String htmlCode = client.DownloadString(HtmlURL);
                StringBuilder Sb = new StringBuilder(htmlCode);
                System.Text.RegularExpressions.Regex regHeaders = new Regex(PDFConstants.Instance.RegularExpressionHeader, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var replaced = regHeaders.Replace(Sb.ToString(), new MatchEvaluator(EvaluateHeaders));
                string filePath = HtmlURL;
                //Here Find <span> Tag and create Index point
                StringBuilder SbIndex = new StringBuilder(replaced + PDFConstants.Instance.HTMLPageBreakTag + PDFConstants.Instance.HTMLCreateIndexTag);

                Regex regexIndex = new Regex(PDFConstants.Instance.RegularExpressionIndex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                MatchCollection matches = regexIndex.Matches(replaced);
                foreach (Match match in matches)
                {
                    Regex rgx = new Regex("[^a-zA-Z ]");
                    string str = rgx.Replace(match.Groups["content"].Value.ToString().Trim().Replace("&nbsp;", ""), "");
                    string str1 = rgx.Replace(match.Groups["titlename"].Value.ToString().Trim().Replace("&nbsp;", ""), "");

                    if (!string.IsNullOrEmpty(str1))
                    {
                        lstTemp.Add(str1);
                        Old_lstTemp.Add(str1);
                    }
                    else
                    {
                        lstTemp.Add(str);
                        Old_lstTemp.Add(str);
                    }
                }

                lstTemp.Sort();


                var replacedIndex = regexIndex.Replace(SbIndex.ToString(), EvaluateIndex);

                //Here Index Create and Add
                StringBuilder SbIndexEntry = new StringBuilder(replacedIndex);
                Regex regexIndexEntry = new Regex(PDFConstants.Instance.RegularExpressionCreateIndex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                countTOC++;
                TOC += string.Format(PDFConstants.Instance.HTMLTOCH1Tag, countTOC, "Index");
                var replacedIndexEntry = regexIndexEntry.Replace(SbIndexEntry.ToString(), Index + string.Format(PDFConstants.Instance.HTMLPageBreakTag + PDFConstants.Instance.HTMLReplaceTOCH1Tag, countTOC, "Index") + PDFConstants.Instance.HTMLTableEndTag);
                 int totalindexcount = 0;
                totalindexcount = (NewIndexList.Count - 1);

                using (FileStream fs = new FileStream(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), "temp.html"), FileMode.Create))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fs, Encoding.UTF8))
                    {
                        streamWriter.WriteLine(TOC + PDFConstants.Instance.HTMLTableEndTag + PDFConstants.Instance.HTMLPageBreakTag);
                        streamWriter.WriteLine(replacedIndexEntry);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
                
               CreatePDF(PDFPath);
                #endregion
           }
        #region Methods To Generate TOC
        private string EvaluateHeaders(Match m)
        {
            switch (int.Parse(m.Groups["header"].Value))
            {
                case 1: // h1
                    countTOC++;
                    TOC += string.Format(PDFConstants.Instance.HTMLTOCH1Tag,countTOC, m.Groups["text"].Value.ToString());
                    return string.Format(PDFConstants.Instance.HTMLPageBreakTag + PDFConstants.Instance.HTMLReplaceTOCH1Tag, countTOC, m.Groups["text"].Value.ToString());

                case 2: // h2
                    countTOC++;
                    TOC += string.Format(PDFConstants.Instance.HTMLTOCH2Tag, countTOC, m.Groups["text"].Value.ToString());
                    return string.Format( PDFConstants.Instance.HTMLReplaceTOCH2Tag, countTOC, m.Groups["text"].Value.ToString());

                case 3: // h3
                    countTOC++;
                    TOC += string.Format(PDFConstants.Instance.HTMLTOCH3Tag, countTOC, m.Groups["text"].Value.ToString());
                    return string.Format(PDFConstants.Instance.HTMLReplaceTOCH3Tag, countTOC, m.Groups["text"].Value.ToString());

                default:
                    TOC += "";
                    return m.Value;
            }

        }

        #region Function for create Index
        private string EvaluateIndex(Match m)
        {
            string result = "";
            Regex rgx = new Regex("[^a-zA-Z ]");
            string str1 = rgx.Replace(m.Groups["titlename"].Value.ToString().Trim(), "");
            int itemIdold = 0;
            List<string> newList = new List<string>();
            if (countIndex != lstTemp.Count)
            {
                if (!newList.Contains(m.Groups["content"].Value.ToString().Trim()))
                {
                    if (lstTemp[countIndex] != (countIndex == 0 ? string.Empty : lstTemp[countIndex - 1]))
                    {
                        itemIdold = Old_lstTemp.FindIndex(a => a == lstTemp[countIndex].ToString());
                        int itemId = lstTemp.FindIndex(a => a == lstTemp[countIndex].ToString());
                        if (lstTemp[countIndex].ToString().ToUpper().Trim().Substring(0, 1) != (countIndex == 0 ? string.Empty : lstTemp[countIndex - 1].ToString().ToUpper().Trim().Substring(0, 1)))
                        {
                            Index += string.Format(PDFConstants.Instance.HTMLTROpenCloseTag,lstTemp[countIndex].ToString().ToUpper().Trim().Substring(0, 1));
                        }
                        Index += string.Format(PDFConstants.Instance.HTMLIndexContent, (itemIdold + 1), lstTemp[countIndex].ToString());
                        NewIndexList.Add(itemIdold + 1);

                    }
                    if (m.Groups["classid"].Value.ToString().Trim().Equals("h1\""))
                    {
                        result = string.Format( PDFConstants.Instance.HTMLSPANTagforH1TargetId, (countIndex + 1).ToString() , str1, m.Groups["content"].Value.ToString());
                    }
                    else if (m.Groups["classid"].Value.ToString().Trim().Equals("h2\""))
                    {
                        result = string.Format(PDFConstants.Instance.HTMLSPANTagforH2TargetId, (countIndex + 1).ToString(), str1, m.Groups["content"].Value.ToString());
                    }
                    else
                    {
                        result = string.Format(PDFConstants.Instance.HTMLSPANTagforH3TargetId, (countIndex + 1).ToString(), str1, m.Groups["content"].Value.ToString());
                    }
                    newList.Add(m.Groups["content"].Value.ToString());
                }
                else
                {
                    if (m.Groups["classid"].Value.ToString().Trim().Equals("h1\""))
                    {
                        result = string.Format(PDFConstants.Instance.HTMLSPANTagforH1, str1,m.Groups["content"].Value.ToString());
                    }
                    else if (m.Groups["classid"].Value.ToString().Trim().Equals("h2\""))
                    {
                        result = string.Format(PDFConstants.Instance.HTMLSPANTagforH2, str1, m.Groups["content"].Value.ToString());
                    }
                    else
                    {
                        result = string.Format(PDFConstants.Instance.HTMLSPANTagforH3, str1, m.Groups["content"].Value.ToString());
                    }
                }
                countIndex++;
            }
            return result;
        }
        #endregion

         /// <summary>
         /// Function Creates PDF from HTML WITH TOC and Header-Footer
         /// </summary>
         /// <param name="PDFPath"></param>
        private void CreatePDF(string PDFPath)
        {
            #region Private Variables
            PdfConverter pdfConverter = new PdfConverter();
            int mappingsTableIdx = 0;
            Document document = new Document(); //New document to be used for adding blank pages
            Document mergedDocument = new Document(); //New Document to merge Header and Pdfdocument
            string coverImg = string.Empty;
            float tocEntryMaxRight = 0.0f;
            int cntAddIndxForPage = 0;
            List<string> lstHeader = new List<string>();
            int pageNum = 0;
            string strHeader = string.Empty;
            List<int> lstBlankPage = new List<int>();
            #endregion           
            // show the bookmarks when the document is opened
            pdfConverter.PdfViewerPreferences.PageMode = ViewerPageMode.UseNone;
            // set page margins
            pdfConverter.PdfDocumentOptions.TopMargin = 20;
            pdfConverter.PdfDocumentOptions.BottomMargin = 20;
            pdfConverter.PdfDocumentOptions.LeftMargin = 20;
            pdfConverter.PdfDocumentOptions.RightMargin = 20;
            pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter11x17;
            pdfConverter.PdfDocumentOptions.FitWidth = false;
            pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
            pdfConverter.AvoidImageBreak = true;
            pdfConverter.AvoidTextBreak = true;
            // Inform the converter about the HTML elements for which we want the location in PDF
            // The HTML ID of each entry in the table of contents is of form TOCEntry_{EntryIndex}_ID
            // the HTML ID of each target of a table of contents entry is of form TOCEntry_{EntryIndex}_Target_ID
            // Both toc entries and toc entries targets locations in PDF will be retrieved
            // and therefore the number of IDs is twice TOC entries number
            pdfConverter.HtmlElementsMappingOptions.HtmlElementSelectors = new string[2 * countTOC];
                       
            for (int tocEntryIndex = 1; tocEntryIndex <= countTOC; tocEntryIndex++)
            {
                // add the HTML ID of the TOC entry element to the list of elements for which we want the PDF location
                string tocEntryID = String.Format(PDFConstants.Instance.TOCENTRYID, tocEntryIndex);
                pdfConverter.HtmlElementsMappingOptions.HtmlElementSelectors[mappingsTableIdx++] = tocEntryID;
                // add the HTML ID of the TOC entry target element to the list of elements for which we want the PDF location
                string tocEntryTargetID = String.Format(PDFConstants.Instance.TOCTARGETID , tocEntryIndex);
                pdfConverter.HtmlElementsMappingOptions.HtmlElementSelectors[mappingsTableIdx++] = tocEntryTargetID;
            }
            // set bookmark options
            pdfConverter.PdfBookmarkOptions.HtmlElementSelectors = new string[] { PDFConstants.Instance.HTMLBookmarkTag };

            // the URL of the HTML document to convert
            string thisPageURL = HttpContext.Current.Request.Url.AbsoluteUri;
            string htmlBookFilePath = Path.Combine(HttpContext.Current.Server.MapPath("~/data"), "temp.html");

            // show header in rendered PDF
            pdfConverter.PdfDocumentOptions.ShowHeader = true;
            
            //Add pages to header Document
            coverImg = Path.Combine(HttpContext.Current.Server.MapPath("~/Images"), "Cover.jpg");
            Document header = pdfConverter.GetPdfDocumentObjectFromHtmlString(string.Format(PDFConstants.Instance.HTMLCoverImageTag,coverImg ) +
                                 PDFConstants.Instance. HTMLMETAContentTag);

            // call the converter and get a Document object from URL
            Document pdfDocument = pdfConverter.GetPdfDocumentObjectFromUrl(htmlBookFilePath);
            pdfDocument.ViewerPreferences.CenterWindow = true;

            // Create a font used to write the page numbers in the table of contents
            PdfFont pageNumberFont = pdfDocument.Fonts.Add(new Font("Times New Roman", PAGE_NUMBER_FONT_SIZE, FontStyle.Regular, GraphicsUnit.Point), true);

            #region Generate TOC & Add to PDF

            // get the right edge of the table of contents where to position the page numbers            
            for (int tocEntryIdx = 1; tocEntryIdx <= countTOC; tocEntryIdx++)
            {
                string tocEntryID = String.Format("TOCEntry_{0}_ID", tocEntryIdx);
                HtmlElementMapping tocEntryLocation = pdfConverter.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByHtmlId(tocEntryID);
                if (tocEntryLocation != null)
                {
                    if (tocEntryLocation.PdfRectangles[0].Rectangle.Right > tocEntryMaxRight)
                        tocEntryMaxRight = tocEntryLocation.PdfRectangles[0].Rectangle.Right;
                }
            }            

            // Add page number for each entry in the table of contents
            for (int tocEntryIdx = 1; tocEntryIdx <= countTOC; tocEntryIdx++)
            {
                string tocEntryID = String.Format("TOCEntry_{0}_ID", tocEntryIdx);
                string tocEntryTargetID = String.Format("TOCEntry_{0}_Target_ID", tocEntryIdx);

                HtmlElementMapping tocEntryLocation = pdfConverter.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByHtmlId(tocEntryID);
                HtmlElementMapping tocEntryTargetLocation = pdfConverter.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByHtmlId(tocEntryTargetID);

               // get the TOC entry page and bounds
                PdfPage tocEntryPdfPage = pdfDocument.Pages[tocEntryLocation.PdfRectangles[0].PageIndex];
                RectangleF tocEntryPdfRectangle = tocEntryLocation.PdfRectangles[0].Rectangle;

                if (tocEntryLocation.HtmlElementCssClassName.Equals("h1"))
                {
                    while (pageNum < tocEntryTargetLocation.PdfRectangles[0].PageIndex + 1 + cntAddIndxForPage)
                    {
                        pageNum++;
                        lstHeader.Add(strHeader);
                    }

                    if ((tocEntryTargetLocation.PdfRectangles[0].PageIndex + 1 + cntAddIndxForPage) % 2 == 0)
                    {
                        pdfDocument.Pages.Insert(tocEntryTargetLocation.PdfRectangles[0].PageIndex + cntAddIndxForPage, document.AddPage(PageSize.Letter11x17, Margins.Empty));
                        lstBlankPage.Add(tocEntryTargetLocation.PdfRectangles[0].PageIndex + cntAddIndxForPage + 1);
                        cntAddIndxForPage++;
                    }
                    strHeader = tocEntryLocation.HtmlElementText;
                }
               // get the page number of target where the TOC entry points
                int tocEntryTargetPageNumber = tocEntryTargetLocation.PdfRectangles[0].PageIndex + 1 + cntAddIndxForPage;

                // add dashed line from text entry to the page number
                LineElement lineToNumber = new LineElement(tocEntryPdfRectangle.Right + 5, tocEntryPdfRectangle.Y + tocEntryPdfRectangle.Height / 1.2F,
                        tocEntryMaxRight + 200, tocEntryPdfRectangle.Y + tocEntryPdfRectangle.Height / 1.2F);
                lineToNumber.LineStyle.LineWidth = 1;
                lineToNumber.LineStyle.LineDashStyle = LineDashStyle.Dot;
                lineToNumber.ForeColor = Color.Black;                
                tocEntryPdfPage.AddElement(lineToNumber);
                
                // create the page number text element to the right of the TOC entry
                TextElement pageNumberTextEement = new TextElement(tocEntryMaxRight + 205, tocEntryPdfRectangle.Y, -1, tocEntryPdfRectangle.Height,
                tocEntryTargetPageNumber.ToString(), pageNumberFont);
                pageNumberTextEement.TextAlign = HorizontalTextAlign.Left;
                pageNumberTextEement.VerticalTextAlign = VerticalTextAlign.Middle;
                pageNumberTextEement.ForeColor = Color.Black;
                // add the page number to the right of the TOC entry
                tocEntryPdfPage.AddElement(pageNumberTextEement);
            }

            #endregion

            #region Create index at End of the Document

            int TOC_INDEX_COUNT = NewIndexList.Count;
            pdfConverter.HtmlElementsMappingOptions.HtmlElementSelectors = new string[2 * TOC_INDEX_COUNT];

            int mappingsEntryTableIdx = 0;
            for (int tocEntryIndex = 1; tocEntryIndex <= TOC_INDEX_COUNT; tocEntryIndex++)
            {
                // add the HTML ID of the TOC entry element to the list of elements for which we want the PDF location
                string tocEntryID = String.Format("#TOCIndex_{0}_ID", NewIndexList[tocEntryIndex - 1].ToString());
                pdfConverter.HtmlElementsMappingOptions.HtmlElementSelectors[mappingsEntryTableIdx++] = tocEntryID;

                string tocEntryTargetID = String.Format("#TOCIndex_{0}_Target_ID", NewIndexList[tocEntryIndex - 1].ToString());
                pdfConverter.HtmlElementsMappingOptions.HtmlElementSelectors[mappingsEntryTableIdx++] = tocEntryTargetID;
            }

            // call the converter and get a Document object from URL
            Document pdfDocumentI = pdfConverter.GetPdfDocumentObjectFromUrl(htmlBookFilePath);

            // Create a font used to write the page numbers in the table of contents
            PdfFont pageNumberFontI = pdfDocument.Fonts.Add(new Font("Arial", PAGE_NUMBER_FONT_SIZE, FontStyle.Bold, GraphicsUnit.Point), true);

            // get the right edge of the Index where to position the page numbers
            float tocEntryMaxRightI = 0.0f;
            for (int tocEntryIdx = 1; tocEntryIdx <= TOC_INDEX_COUNT; tocEntryIdx++)
            {
                string tocEntryID = String.Format("TOCIndex_{0}_ID", tocEntryIdx);
                HtmlElementMapping tocEntryLocation = pdfConverter.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByHtmlId(tocEntryID);
                if (tocEntryLocation != null)
                {
                    if (tocEntryLocation.PdfRectangles[0].Rectangle.Right > tocEntryMaxRightI)
                        tocEntryMaxRightI = tocEntryLocation.PdfRectangles[0].Rectangle.Right;
                }
            }
            // Add page number for each entry in the Index
            for (int tocEntryIdx = 1; tocEntryIdx <= TOC_INDEX_COUNT; tocEntryIdx++)
            {
                string tocEntryID = String.Format("TOCIndex_{0}_ID", NewIndexList[tocEntryIdx - 1].ToString());
                string tocEntryTargetID = String.Format("TOCIndex_{0}_Target_ID", NewIndexList[tocEntryIdx - 1].ToString());

                HtmlElementMapping tocEntryLocation = pdfConverter.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByHtmlId(tocEntryID);
                HtmlElementMapping tocEntryTargetLocation = pdfConverter.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByHtmlId(tocEntryTargetID);

                // get the TOC entry page and bounds
                PdfPage tocEntryPdfPage = pdfDocument.Pages[tocEntryLocation.PdfRectangles[0].PageIndex + cntAddIndxForPage];
                RectangleF tocEntryPdfRectangle = tocEntryLocation.PdfRectangles[0].Rectangle;

                // get the page number of target where the TOC entry points
                int tocEntryTargetPageNumber = tocEntryTargetLocation.PdfRectangles[0].PageIndex + 1;

                for (int i = 0; i < lstBlankPage.Count; i++)
                {
                    if (lstBlankPage[i] <= tocEntryTargetPageNumber)
                    {
                        tocEntryTargetPageNumber++;
                    }
                }

                // add dashed line from text entry to the page number
                LineElement lineToNumber = new LineElement(tocEntryPdfRectangle.Right + 5, tocEntryPdfRectangle.Y + tocEntryPdfRectangle.Height / 1.2F,
                        tocEntryMaxRight, tocEntryPdfRectangle.Y + tocEntryPdfRectangle.Height / 1.2F);
                lineToNumber.LineStyle.LineWidth = 1;
                lineToNumber.LineStyle.LineDashStyle = LineDashStyle.Dot;
                lineToNumber.ForeColor = Color.Black;
                tocEntryPdfPage.AddElement(lineToNumber);

                // create the page number text element to the right of the TOC entry
                TextElement pageNumberTextEement = new TextElement(tocEntryMaxRight + 5, tocEntryPdfRectangle.Y, -1, tocEntryPdfRectangle.Height,
                tocEntryTargetPageNumber.ToString(), pageNumberFont);
                pageNumberTextEement.TextAlign = HorizontalTextAlign.Left;
                pageNumberTextEement.VerticalTextAlign = VerticalTextAlign.Middle;
                pageNumberTextEement.ForeColor = Color.Black;

                // add the page number to the right of the TOC entry
                tocEntryPdfPage.AddElement(pageNumberTextEement);
            }
            #endregion

            #region Set Header On Each Pages

            // the width is given by the PDF page width
            float altHeaderFooterWidth = pdfDocument.Pages[0].ClientRectangle.Width;
            float altHeaderHeight = pdfConverter.PdfHeaderOptions.HeaderHeight;

            //Add Header Chapter for last chapter in Header list
            for (int pages = lstHeader.Count; pages < pdfDocument.Pages.Count; pages++)
            {
                lstHeader.Add(strHeader);
            }

            //Remove Unnecessary Header Line from Header Document
            for (int pageIndex = 0; pageIndex < header.Pages.Count; pageIndex++)
            {
                Template tmpHeaderTemplate = pdfDocument.Templates.AddNewTemplate(altHeaderFooterWidth, altHeaderHeight);
                PdfPage pdfPage = header.Pages[pageIndex];
                pdfPage.CustomHeaderTemplate = tmpHeaderTemplate;
                pdfPage.ShowHeaderTemplate = true;
            }

            //Iterate through pages and add header details on each page.. i.e. Chapter Name, Page Number
            for (int pageIndex = 0; pageIndex < pdfDocument.Pages.Count; pageIndex++)
            {
                // create the alternate header template
                Template altHeaderTemplate = pdfDocument.Templates.AddNewTemplate(altHeaderFooterWidth, altHeaderHeight);
                TextElement txtHeader = new TextElement(10, altHeaderHeight, lstHeader[pageIndex].ToString(), pageNumberFont, Color.Black);
                altHeaderTemplate.AddElement(txtHeader);

                TextElement txtPageNumber = new TextElement(altHeaderFooterWidth - 50, altHeaderHeight, (pageIndex + 1).ToString(), pageNumberFont, Color.Black);
                altHeaderTemplate.AddElement(txtPageNumber);

                PdfPage pdfPage = pdfDocument.Pages[pageIndex];
                pdfPage.CustomHeaderTemplate = altHeaderTemplate;
                pdfPage.ShowHeaderTemplate = true;
            }

            #endregion

            mergedDocument.AppendDocument(header);
            mergedDocument.AppendDocument(pdfDocument);

           try
            {
                mergedDocument.Save(PDFPath);
            }
            finally
            {
                // close the Documents to realease all the resources
                mergedDocument.Close();
                header.Close();
                pdfDocument.Close();
                document.Close();
                pdfDocumentI.Close();
                lstBlankPage.Clear();
                TOC_INDEX_COUNT = 0;
                FreeResources();
            }
        }

        private void FreeResources()
        {
            if (File.Exists(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), "temp.html")))
            {
                File.Delete(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), "temp.html"));
            }
            if (File.Exists(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), "temp.html")))
            {
                File.Delete(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), "temp.html"));
            }
            if (File.Exists(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), Path.GetFileNameWithoutExtension(PDFPath) + ".html")))
            {
                File.Delete(Path.Combine(HttpContext.Current.Server.MapPath("~/data"), Path.GetFileNameWithoutExtension(PDFPath) + ".html"));
            }
           
            countTOC = 0;
            TOC = string.Empty;
            NewIndexList.Clear();
            lstTemp.Clear();
            Old_lstTemp.Clear();
            countIndex = 0;
            Index = string.Empty;
            GC.Collect();
        }
        #endregion
      /// <summary>
        /// Create a PdfConverter object
        /// </summary>
        /// <returns></returns>
        private PdfConverter GetPdfConverter()
        {
            PdfConverter pdfConverter = new PdfConverter();
            return pdfConverter;
        }

    }
}

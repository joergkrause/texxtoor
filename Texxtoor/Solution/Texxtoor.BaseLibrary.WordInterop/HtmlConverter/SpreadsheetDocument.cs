/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO.Packaging;
using OpenXmlSDK = DocumentFormat.OpenXml.Packaging;
using System.Xml.Linq;
using OpenXml.PowerTools.SpreadSheet;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Represents a Spreadsheet Document and exposes functionality to manipulate content
    /// </summary>
    public class SpreadsheetDocument : OpenXmlDocument
    {
        /// <summary>
        /// Provides access to worksheet operations
        /// </summary>
        public WorksheetAccessor Worksheets { get; set; }

        /// <summary>
        /// Provides access to chart sheet operations
        /// </summary>
        public ChartsheetAccessor Chartsheets { get; set; }

        /// <summary>
        /// Provides access to chart sheet operations
        /// </summary>
        public CustomPropertiesAccesor CustomProperties { get; set; }
        /// <summary>
        /// Style get/set
        /// </summary>
        public SpreadSheetStyleAccessor Style { get; set; }
        /// <summary>
        /// Provides access to tables defined in workbook sheets
        /// </summary>
        public SpreadSheetTableAccesor Tables { get; set; }

        /// <summary>
        /// Document to perform operations on
        /// </summary>
        public new OpenXmlSDK.SpreadsheetDocument Document
        {
            get { return (OpenXmlSDK.SpreadsheetDocument)base.Document; }
            set { base.Document = value; }
        }

        /// <summary>
        /// InnerContent
        /// </summary>
        public new SpreadsheetDocumentManager InnerContent
        {
            get { return (SpreadsheetDocumentManager)base.InnerContent; }
            set { base.InnerContent = value; }
        }
        /// <summary>
        /// Creates a new SpreadsheetDocument from an existing file
        /// </summary>
        /// <param name="filePath">Path of file to load</param>
        public SpreadsheetDocument(string filePath) : this(filePath, false) { }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        /// <param name="fullName">Full path name of document</param>
        public SpreadsheetDocument(OpenXmlSDK.SpreadsheetDocument document, string fullName)
            : base()
        {
            Document = document;
            FullName = fullName;
            InnerContent = new SpreadsheetDocumentManager(this);
            Worksheets = new WorksheetAccessor(this);
            Chartsheets = new ChartsheetAccessor(this);
            CustomProperties = new CustomPropertiesAccesor(this);
            Tables = new SpreadSheetTableAccesor(this);
            Style = new SpreadSheetStyleAccessor(this);
        }

        /// <summary>
        /// AcceptAllChanges
        /// </summary>
        public override void AcceptAllChanges()
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// RejectAllChanges
        /// </summary>
        public override void RejectAllChanges()
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// SetBackgroundImage
        /// </summary>
        public override void SetBackgroundImage(string bgImagePath)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// GetBackgroundImage
        /// </summary>
        public override string GetBackgroundImage()
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// GetAllComments
        /// </summary>
        public override object GetAllComments(CommentFormat format)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// FindCustomXml
        /// </summary>
        public override XDocument FindCustomXml(string partName)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// GetStylesDocument
        /// </summary>
        public override XDocument GetStylesDocument()
        {
            return Style.GetStylesDocument();
        }
        /// <summary>
        /// GetTheme
        /// </summary>
        public override Package GetTheme(string outputPath)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// InsertFormat
        /// </summary>
        public override void InsertFormat(string xpathInsertionPoint, string xmlContent)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// InsertStyle
        /// </summary>
        public override void InsertStyle(string xpathInsertionPoint, string styleName, XDocument stylesSource)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// SetCustomXmlDocument
        /// </summary>
        public override void SetCustomXmlDocument(XDocument data, string partName)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// SetStylePart
        /// </summary>
        public override void SetStylePart(XDocument styleDocument)
        {
            Style.SetStylePart(styleDocument);
        }
        /// <summary>
        /// SetTheme
        /// </summary>
        public override void SetTheme(Package themePackage)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// InsertPicture
        /// </summary>
        public override void InsertPicture(string insertionPoint, System.Drawing.Image image, string name)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// JoinDocuments
        /// </summary>
        public override OpenXmlDocument JoinDocuments(IEnumerable<OpenXmlDocument> documents, string outputFilePath)
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// RemoveAllComments
        /// </summary>
        public override void RemoveAllComments()
        {
            throw new InvalidOperationException("Operation not yet supported for Spreadsheets.");
        }
        /// <summary>
        /// Set Table Style for cell range
        /// </summary>
        /// <param name="worksheetName">Worksheet name to add the table to</param>
        /// <param name="tableStyle">Style to be assigned to the table</param>
        /// <param name="useHeaders">Set a header row</param>
        /// <param name="fromColumn">Table initial column</param>
        /// <param name="toColumn">Table final column</param>
        /// <param name="fromRow">Table initial row</param>
        /// <param name="toRow">Table final row</param>
        public override void AddTable(string worksheetName, string tableStyle, bool useHeaders, short fromColumn, short toColumn, int fromRow, int toRow)
        {
            Tables.Add(Worksheets.Get(worksheetName), tableStyle, useHeaders, fromColumn, toColumn, fromRow, toRow);
        }

        /// <summary>
        /// Setting value for a spreadsheet cell
        /// <remarks>Author:Johann Granados Company: Staff DotNet Creation Date: 8/30/2008</remarks>
        /// </summary>
        public override void SetCellValue(string worksheetName, int fromRow, int toRow, short fromColumn, short toColumn, string value)
        {
            WorksheetAccessor.SetCellValue(Worksheets.Get(worksheetName), fromRow, toRow, fromColumn, toColumn, value);
        }

        /// <summary>
        /// Setting value for a spreadsheet cell
        /// <remarks>Author:Johann Granados Company: Staff DotNet Creation Date: 8/30/2008</remarks>
        /// </summary>
        public override void SetCellStyle(string worksheetName, int fromRow, int toRow, short fromColumn, short toColumn, string cellStyle)
        {
            WorksheetAccessor.SetCellStyle(Worksheets.Get(worksheetName), fromColumn, toColumn, fromRow, toRow, cellStyle);
        }

        /// <summary>
        /// Setting width for a range of columns
        /// <remarks>Author:Johann Granados Company: Staff DotNet Creation Date: 8/30/2008</remarks>
        /// </summary>
        public override void SetColumnWidth(string worksheetName, short fromColumn, short toColumn, int width)
        {
            WorksheetAccessor.SetColumnWidth(Worksheets.Get(worksheetName), fromColumn, toColumn, width);
        }

        /// <summary>
        /// Creates a new SpreadsheetDocument from a new or existing file
        /// </summary>
        /// <param name="filePath">Path of file</param>
        /// <param name="createNew">Whether create a new document or load from an existing one</param>
        public SpreadsheetDocument(string filePath, bool createNew)
        {
            try
            {
                if (createNew)
                    Document = OpenXmlSDK.SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                else
                    Document = OpenXmlSDK.SpreadsheetDocument.Open(filePath, true);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
            }

            FullName = filePath;
            InnerContent = new SpreadsheetDocumentManager(this);
            Worksheets = new WorksheetAccessor(this);
            Chartsheets = new ChartsheetAccessor(this);
            CustomProperties = new CustomPropertiesAccesor(this);
        }

        /// <summary>
        /// Creates a SpreadsheetDocument with a chart from a data table
        /// </summary>
        /// <param name="outputPath">Path of generated file</param>
        /// <param name="headerList">Header row contents</param>
        /// <param name="valueTable">Data values</param>
        /// <param name="chartType">Chart type</param>
        /// <param name="headerColumn">Column to use as category for charting</param>
        /// <param name="columnsToChart">Columns to include in chart</param>
        /// <param name="initialRow">Row index to start copying data</param>
        /// <returns>Spreadsheet document</returns>
        public static SpreadsheetDocument Create(string outputPath, List<string> headerList, string[][] valueTable, ChartType chartType, string headerColumn, List<string> columnsToChart, int initialRow)
        {
            return SpreadsheetDocumentManager.Create(outputPath, headerList, valueTable, chartType, headerColumn, columnsToChart, initialRow);
        }

        /// <summary>
        /// Creates a SpreadsheetDocument from a data table
        /// </summary>
        /// <param name="outputPath">Path of generated file</param>
        /// <param name="headerList">Header row contents</param>
        /// <param name="valueTable">Data values</param>
        /// <param name="initialRow">Row index to start copying data</param>
        /// <returns>Spreadsheet document</returns>
        public static SpreadsheetDocument Create(string outputPath, List<string> headerList, string[][] valueTable, int initialRow)
        {
            return SpreadsheetDocumentManager.Create(outputPath, headerList, valueTable, initialRow);
        }
    }
}
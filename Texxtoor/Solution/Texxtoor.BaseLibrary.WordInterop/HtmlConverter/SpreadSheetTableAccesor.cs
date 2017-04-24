/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/
/**************************************************************************
Author          : Johann Granados
Company         : Staff DotNet
Email           : johann.granados@staffdotnet.com
Blog            : http://blogs.staffdotnet.com/johanngranados
Creation Date   : 9/2/2008
**************************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenXmlSDK = DocumentFormat.OpenXml.Packaging;
using System.IO.Packaging;
using System;
using System.Xml;

namespace OpenXml.PowerTools
{
    /// <summary>
    /// Class for working with the Table Definition Part for a SpreadsheetML document
    /// </summary>
    public class SpreadSheetTableAccesor
    {
        private static XNamespace ns;
        private static XNamespace relationshipns;
        private SpreadsheetDocument parentDocument;
        private List<OpenXmlSDK.TableDefinitionPart> tables = new List<OpenXmlSDK.TableDefinitionPart>();

        /// <summary>
        /// Static constructor
        /// </summary>
        static SpreadSheetTableAccesor()
        {
            ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            relationshipns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";    
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="parent">Open Xml Document containing this table definition part</param>
        public SpreadSheetTableAccesor(SpreadsheetDocument parent)
        {
            parentDocument = parent;
        }

        /// <summary>
        /// Method for adding a new table definition part
        /// </summary>
        /// <param name="worksheet">Worksheet to add the table to</param>
        /// <param name="tableStyle">Style to be assigned to the table</param>
        /// <param name="useHeaders">Set a header row</param>
        /// <param name="fromColumn">Initial column for table</param>
        /// <param name="toColumn">Final column for table</param>
        /// <param name="fromRow">Intial row for table</param>
        /// <param name="toRow">Final row for table</param>
        public void Add(OpenXmlSDK.WorksheetPart worksheet, string tableStyle, bool useHeaders, short fromColumn, short toColumn, int fromRow, int toRow)
        {
            //Getting the id for this table
            int tableId = GetNextTableId();
            
            //Set the table cell range
            string tableRange = string.Format("{0}{1}:{2}{3}", WorksheetAccessor.GetColumnId(fromColumn),
                                                              fromRow,
                                                              WorksheetAccessor.GetColumnId(toColumn),
                                                              toRow);

            //Creating a new id for the relationship between the table definition part and the worksheet
            string tableRelationShipId = "rId" + Guid.NewGuid();

            //Create a new table definition part
            OpenXmlSDK.TableDefinitionPart table = worksheet.AddNewPart<OpenXmlSDK.TableDefinitionPart>(tableRelationShipId);

            //string tableColumns = string.Empty;
            XElement tableColumnsXElement = new XElement(ns + "tableColumns",new XAttribute("count",(toColumn - fromColumn)+1));
            //Get the name for table column elements from the first table row
            string[] tableHeaders = GetTableHeaders(worksheet,fromRow,fromColumn,toColumn);
            for (int i = 0; i <= (toColumn-fromColumn); i++)
            {
                //Create the markup for the SpreadsheetML table column elements
                tableColumnsXElement.Add(
                    new XElement(ns + "tableColumn",new XAttribute("id",i+1),new XAttribute("name",tableHeaders[i])));

            }

            XElement tableXElement = 
                new XElement(ns + "table",
                    new XAttribute("xmlns",ns), //default namespace
                    new XAttribute("id",tableId),
                    new XAttribute("name","Table" + tableId.ToString()),
                    new XAttribute("displayName","Table" + tableId.ToString()),
                    new XAttribute("ref",tableRange),
                    new XAttribute("totalsRowShown","0"));

            if (useHeaders)
            {
                tableXElement.Add(
                    new XElement(ns + "autoFilter", new XAttribute("ref", tableRange)));
            }

            tableXElement.Add(tableColumnsXElement);

            tableXElement.Add(
                new XElement(ns + "tableStyleInfo",
                new XAttribute("name",tableStyle),
                new XAttribute("showFirstColumn","0"),
                new XAttribute("showLastColumn","0"),
                new XAttribute("showRowStripes","0"),
                new XAttribute("showColumnStripes","0")));

            //Write the markup to the Table Definition Part Stream
            XmlWriter tablePartStreamWriter = XmlWriter.Create(table.GetStream());
            tableXElement.WriteTo(tablePartStreamWriter);
            
            tablePartStreamWriter.Flush();
            tablePartStreamWriter.Close();

            //Create or modify the table parts definition at worksheet (for setting the relationship id with the new table)
            XDocument worksheetMarkup = parentDocument.GetXDocument(worksheet);
            //Look for the tableParts element at worksheet markup
            XElement tablePartsElement = worksheetMarkup.Root.Element(ns + "tableParts");
            if (tablePartsElement != null)
            {   
                //tableParts elements does exist at worksheet markup
                //increment the tableParts count attribute value
                short tableCount = System.Convert.ToInt16(tablePartsElement.Attribute("count").Value);
                tablePartsElement.SetAttributeValue("count", tableCount++.ToString());

            }
            else
            {  
                //tableParts does not exist at worksheet markup
                //create a new tableParts element
                tablePartsElement = new XElement(ns + "tableParts",
                                                               new XAttribute(ns + "count", "1"));

                worksheetMarkup.Root.Add(tablePartsElement);
            }
            
            //create the tablePart element
            XElement tablePartEntryElement = new XElement(ns + "tablePart",
                                                              new XAttribute(relationshipns + "id", tableRelationShipId));
            
            //add the new tablePart element to the worksheet tableParts element
            tablePartsElement.Add(tablePartEntryElement);
        }

        /// <summary>
        /// Returns the id for the next table to add to the SpreadSheetML document
        /// </summary>
        /// <returns></returns>
        private int GetNextTableId()
        {
            int tableCount = 0;
            foreach (OpenXmlSDK.WorksheetPart worksheet in parentDocument.Document.WorkbookPart.WorksheetParts)
            {  //Loop the worksheets to sum up the tables defined in each one
                tableCount += worksheet.TableDefinitionParts.Count();
            }
            return ++tableCount;
        }

        /// <summary>
        /// Get the values for the table header row (table initial row)
        /// </summary>
        /// <param name="worksheet">Worksheet where the table is being defined</param>
        /// <param name="row">Table initial row</param>
        /// <param name="fromColumn">Table initial column</param>
        /// <param name="toColumn">Table final column</param>
        /// <returns></returns>
        private string[] GetTableHeaders(OpenXmlSDK.WorksheetPart worksheet, int row, short fromColumn, short toColumn)
        {
            List<string> tableHeaders = new List<string>();
            for (short c = fromColumn; c <= toColumn; c++)
            {
                tableHeaders.Add(WorksheetAccessor.GetValue(worksheet, c, row));
            }

            return tableHeaders.ToArray<string>();
        }
    }
}

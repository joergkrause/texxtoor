/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXml.PowerTools.Wordprocessing
{

    /// <summary>
    /// Available categories of references for a TOA
    /// </summary>
    public enum ReferenceCategory
    {
        /// <summary>
        /// Cases
        /// </summary>
        Cases = 1,
        /// <summary>
        /// Statutes
        /// </summary>
        Statutes,
        /// <summary>
        /// OtherAuthorities
        /// </summary>
        OtherAuthorities,
        /// <summary>
        /// Rules
        /// </summary>
        Rules,
        /// <summary>
        /// Treatises
        /// </summary>
        Treatises,
        /// <summary>
        /// Regulations
        /// </summary>
        Regulations,
        /// <summary>
        /// ConstitutionalProvisions
        /// </summary>
        ConstitutionalProvisions,
        /// <summary>
        /// C8
        /// </summary>
        C8,
        /// <summary>
        /// C9
        /// </summary>
        C9,
        /// <summary>
        /// C10
        /// </summary>
        C10,
        /// <summary>
        /// C11
        /// </summary>
        C11,
        /// <summary>
        /// C12
        /// </summary>
        C12,
        /// <summary>
        /// C13
        /// </summary>
        C13,
        /// <summary>
        /// C14
        /// </summary>
        C14,
        /// <summary>
        /// C15
        /// </summary>
        C15,
        /// <summary>
        /// C16
        /// </summary>
        C16
    }

    /// <summary>
    /// Provides methods to work with TOA
    /// </summary>
    public class TOAAccessor
    {
        private PTWordprocessingDocument parentDocument;

        private static XNamespace ns;
        private string TOAFieldPrefix = " TA";
        /// <summary>
        /// CategoryCases
        /// </summary>
        public static readonly string CategoryCases = "Cases";
        /// <summary>
        /// CategoryStatutes
        /// </summary>
        public static readonly string CategoryStatutes = "Statutes";
        /// <summary>
        /// CategoryOtherAuthorities
        /// </summary>
        public static readonly string CategoryOtherAuthorities = "OtherAuthorities";
        /// <summary>
        /// CategoryRules
        /// </summary>
        public static readonly string CategoryRules = "Rules";
        /// <summary>
        /// CategoryTreatises
        /// </summary>
        public static readonly string CategoryTreatises = "Treatises";
        /// <summary>
        /// CategoryRegulations
        /// </summary>
        public static readonly string CategoryRegulations = "Regulations";
        /// <summary>
        /// CategoryConstitutionalProvisions
        /// </summary>
        public static readonly string CategoryConstitutionalProvisions = "ConstitutionalProvisions";
        /// <summary>
        /// Category8
        /// </summary>
        public static readonly string Category8 = "C8";
        /// <summary>
        /// Category9
        /// </summary>
        public static readonly string Category9 = "C9";
        /// <summary>
        /// Category10
        /// </summary>
        public static readonly string Category10 = "C10";
        /// <summary>
        /// Category11
        /// </summary>
        public static readonly string Category11 = "C11";
        /// <summary>
        /// Category12
        /// </summary>
        public static readonly string Category12 = "C12";
        /// <summary>
        /// Category13
        /// </summary>
        public static readonly string Category13 = "C13";
        /// <summary>
        /// Category14
        /// </summary>
        public static readonly string Category14 = "C14";
        /// <summary>
        /// Category15
        /// </summary>
        public static readonly string Category15 = "C15";
        /// <summary>
        /// Category16
        /// </summary>
        public static readonly string Category16 = "C16";

        static TOAAccessor()
        {
            ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">Document to process</param>
        public TOAAccessor(PTWordprocessingDocument document)
        {
            this.parentDocument = document;
        }

        private IEnumerable<XElement> TOAReferences()
        {
            XDocument mainDocument = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            IEnumerable<XElement> results =
              mainDocument.Descendants().Where
              (
                  tag =>
                      tag.Name == ns + "p" &&
                      tag.Descendants().Where
                      (
                        tag2 =>
                            tag2.Name == ns + "instrText" &&
                            (
                                tag2.Value.StartsWith(TOAFieldPrefix)
                            )
                      ).Count() > 0
              );

            return results;
        }

        /// <summary>
        /// Retrieves from a string containing the field-specific-switches of a citation, its
        /// category
        /// </summary>
        /// <param name="reference">A XElement containing a TOA reference</param>
        /// <returns>A string with the field code of the TOA reference</returns>
        private string GetFieldCode(XElement reference)
        {
            //  All the TOA references has a field with parameters describing their attributes. 
            //  one of this parameters specifies the reference category, wich is needed to join
            //  a group of common references

            //  One example of a TOA reference field could be:
            //  TA \l this is some text \s "text" \c 1
            //  the  "\c 2" tells that the category of this reference is the number 2

            //  To retrieve the category, its necesary to first get the field with the parameters
            //  the problem is that the fields string does not come completely together, but separated
            //  with runs and inside a "instrText" element. this "instrText" indicates that this text is a
            //  part of a field code.

            //  Build the complete text of the reference field
            string fieldCode = "";
            foreach (XElement partialFieldCode in reference.Descendants(ns + "instrText"))
            {
                fieldCode += partialFieldCode.Value;
            }

            return fieldCode;
        }

        /// <summary>
        /// Get the reference category of a field code string
        /// </summary>
        /// <param name="fieldCode">the field code string</param>
        /// <returns>The number of the category</returns>
        private int GetReferenceCategory(string fieldCode)
        {
            //  Get the category parameter
            string[] toks = fieldCode.Split(' ');
            int category = 0;
            for (int i = 1; i < toks.Length; i++)
            {
                if (toks[i] == @"\c") // switch parameter for the category type
                {
                    int.TryParse(toks[i + 1], out category); // the value next to the parameter indicates the category value
                    break;
                }
            }
            // if the category is not found it, is necesary to group it in the 'OtherAuthorities' category
            return category;
        }

        /// <summary>
        /// Get the long citation (description or text that goes along with the reference)
        /// </summary>
        /// <param name="fieldCode">the field code</param>
        /// <returns>The category name</returns>
        private string GetLongCitation(string fieldCode)
        {
            //  the regular expression retrieves from the field code the \l parameter and its argument

            Regex expReg = new Regex(@"\\l\s\"".*?(\""\s|$)");
            string longCitation = "";
            Match match = expReg.Match(fieldCode);
            if (match.Success)
            {
                longCitation = match.Value;
            }

            //  remove the '\l' from the long citation

            longCitation = longCitation.Remove(0, 2);

            //remove the " character and the white spaces
            
            longCitation = longCitation.Trim().Substring(1, longCitation.Length - 4);

            return longCitation;
        }

        /// <summary>
        /// For each category number, return the category name, for example the category '1' is the 'cases' category
        /// </summary>
        /// <param name="category">the category number</param>
        /// <returns>The category name</returns>
        private string GetCategoryName(int category)
        {
            switch ((ReferenceCategory)category)
            {
                case ReferenceCategory.Cases: return CategoryCases;
                case ReferenceCategory.Statutes: return CategoryConstitutionalProvisions;
                case ReferenceCategory.Rules: return CategoryRules;
                case ReferenceCategory.Treatises: return CategoryTreatises;
                case ReferenceCategory.Regulations: return CategoryRegulations;
                case ReferenceCategory.ConstitutionalProvisions: return CategoryConstitutionalProvisions;
                case ReferenceCategory.C8: return Category8;
                case ReferenceCategory.C9: return Category9;
                case ReferenceCategory.C10: return Category10;
                case ReferenceCategory.C11: return Category11;
                case ReferenceCategory.C12: return Category12;
                case ReferenceCategory.C13: return Category13;
                case ReferenceCategory.C14: return Category14;
                case ReferenceCategory.C15: return Category15;
                default:
                    return Category16;
            }
        }

        /// <summary>
        /// Generate the TOA (Table Of Authorities) of the document
        /// </summary>
        /// <param name="pos">specify the position of the table in the document</param>
        public void Generate(Position pos)
        {
            XElement TOA = new XElement("TOA");

            foreach (XElement reference in TOAReferences())
            {
                string fieldCode = GetFieldCode(reference);
                int category = GetReferenceCategory(fieldCode);
                string categoryName = GetCategoryName(category);
                string longCitation = GetLongCitation(fieldCode);

                //  The category need to be in the TOA, if it isnt, then it must be added
                if (TOA.Descendants(categoryName).Count() == 0)
                {
                    //  Build the first element of the category part inside the TOA
                    //  this element includes the category table name 
                    XElement TOATitleXml =
                        new XElement(ns + "p",
                            new XElement(ns + "pPr",
                                new XElement(ns + "pStyle",
                                    new XAttribute(ns + "val", "TOAHeading")),
                                new XElement(ns + "tabs",
                                    new XElement(ns + "tab",
                                        new XAttribute(ns + "val", "right"),
                                        new XAttribute(ns + "leader", "dot"),
                                        new XAttribute(ns + "pos", "9350")))),
                            new XElement(ns + "r",
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "begin")),
                                new XElement(ns + "instrText",
                                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                                    @" TOA \h \c """ + category + @""" \p "),
                                new XElement(ns + "fldChar",
                                    new XAttribute(ns + "fldCharType", "separate")),
                                new XElement(ns + "t", categoryName)));

                    TOA.Add(new XElement(categoryName));
                    TOA.Element(categoryName).Add(TOATitleXml);
                }

                //  Build the XElement containing the reference description
                XElement TOAElement =
                    new XElement(ns + "p",
                        new XElement(ns + "pPr",
                            new XElement(ns + "pStyle",
                                new XAttribute(ns + "val", "TableofAuthorities")),
                                new XElement(ns + "tabs",
                                    new XElement(ns + "tab",
                                        new XAttribute(ns + "val", "right"),
                                        new XAttribute(ns + "leader", "dot"),
                                        new XAttribute(ns + "pos", "9350")))),
                        new XElement(ns + "r",
                            new XElement(ns + "t", longCitation)));

                TOA.Element(categoryName).Add(TOAElement);
            }

            //  All the character fields (fldChar) of the TOA category ,elements have not been closed.
            //  Closing the character fields
            foreach (XElement TOAElement in TOA.Elements())
            {
                TOAElement.Add(
                    new XElement(ns + "p",
                        new XElement(ns + "r",
                            new XElement(ns + "fldChar",
                                new XAttribute(ns + "fldCharType", "end")))));
            }

            XDocument mainDocumentPart = parentDocument.GetXDocument(parentDocument.Document.MainDocumentPart);
            foreach(XElement TOAElement in TOA.Elements())
            {
                mainDocumentPart.Descendants(ns + "body").First().Add(TOAElement.Elements());
            }        
        }
    }
}
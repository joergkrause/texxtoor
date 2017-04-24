/***************************************************************************

Copyright (c) Microsoft Corporation 2008.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using System.Security.Cryptography.X509Certificates;
using System.Collections.ObjectModel;

namespace OpenXml.PowerTools {
    /// <summary>
    /// Provides access to digital signature operations
    /// </summary>
    public class DigitalSignatureAccessor
    {
        private OpenXmlDocument parentDocument;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="document">Document to perform operations on</param>
        public DigitalSignatureAccessor(OpenXmlDocument document) {
            parentDocument = document;
        }

       /// <summary>
        /// Digital Signature from a WordprocessingML document package
        /// </summary>
        /// <param name="digitalCertificate"></param>
        public void Insert(string digitalCertificate) {
            X509Certificate x509Certificate = X509Certificate2.CreateFromCertFile(digitalCertificate);
            System.IO.Packaging.PackageDigitalSignatureManager digitalSigntaureManager = new System.IO.Packaging.PackageDigitalSignatureManager(parentDocument.Document.Package);
            digitalSigntaureManager.CertificateOption = System.IO.Packaging.CertificateEmbeddingOption.InSignaturePart;
            System.Collections.Generic.List<Uri> partsToSign = new System.Collections.Generic.List<Uri>();
            //Adds each part to the list, except relationships parts.
            foreach (System.IO.Packaging.PackagePart openPackagePart in parentDocument.Document.Package.GetParts()) {
                if (!System.IO.Packaging.PackUriHelper.IsRelationshipPartUri(openPackagePart.Uri))
                    partsToSign.Add(openPackagePart.Uri);
            }
            List<System.IO.Packaging.PackageRelationshipSelector> relationshipSelectors = new List<System.IO.Packaging.PackageRelationshipSelector>();
            //Creates one selector for each package-level relationship, based on id
            foreach (System.IO.Packaging.PackageRelationship relationship in parentDocument.Document.Package.GetRelationships()) {
                System.IO.Packaging.PackageRelationshipSelector relationshipSelector =
                    new System.IO.Packaging.PackageRelationshipSelector(relationship.SourceUri, System.IO.Packaging.PackageRelationshipSelectorType.Id, relationship.Id);
                relationshipSelectors.Add(relationshipSelector);
            }
            digitalSigntaureManager.Sign(partsToSign, x509Certificate, relationshipSelectors);
        }

        /// <summary>
        ///  Tests a Digital Signature from a package
        /// </summary>
        /// <returns>Digital signatures list</returns>
        public Collection<string> GetList() {
            // Creates the PackageDigitalSignatureManager
            System.IO.Packaging.PackageDigitalSignatureManager digitalSignatureManager = new System.IO.Packaging.PackageDigitalSignatureManager(parentDocument.Document.Package);
            // Verifies the collection of certificates in the package
            Collection<string> digitalSignatureDescriptions = new Collection<string>();
            ReadOnlyCollection<System.IO.Packaging.PackageDigitalSignature> digitalSignatures = digitalSignatureManager.Signatures;
            if (digitalSignatures.Count > 0) {
                foreach (System.IO.Packaging.PackageDigitalSignature signature in digitalSignatures) {
                    if (System.IO.Packaging.PackageDigitalSignatureManager.VerifyCertificate(signature.Signer) != X509ChainStatusFlags.NoError) {
                        digitalSignatureDescriptions.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Signature: {0} ({1})", signature.Signer.Subject, System.IO.Packaging.PackageDigitalSignatureManager.VerifyCertificate(signature.Signer)));
                    }
                    else
                        digitalSignatureDescriptions.Add("Signature: " + signature.Signer.Subject);
                }
            }
            else {
                digitalSignatureDescriptions.Add("No digital signatures found");
            }
            return digitalSignatureDescriptions;
        }

        /// <summary>
        /// RemoveAll
        /// </summary>
        public void RemoveAll()
        {
            // Creates the PackageDigitalSignatureManager
            System.IO.Packaging.PackageDigitalSignatureManager digitalSignatureManager = new System.IO.Packaging.PackageDigitalSignatureManager(parentDocument.Document.Package);
            digitalSignatureManager.RemoveAllSignatures();
        }
    }
}
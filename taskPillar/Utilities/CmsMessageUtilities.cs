using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PillarAPI.Utilities
{
    internal class CmsMessageUtilities
    {
        /// <summary>
        ///     Uses the private key of the certificate to digitaly sign the message
        /// </summary>
        /// <param name="privateCertificate">Certificate containing private key</param>
        /// <param name="messageForSigning">Message for signing</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Text.DecoderFallbackException"></exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns>Signed message</returns>
        public static string CmsMessageSigner(X509Certificate2 privateCertificate, string messageForSigning)
        {
            byte[] messageForSigningInBytes = Encoding.UTF8.GetBytes(messageForSigning);
            var cmsSigner = new CmsSigner(privateCertificate)
                                {
                                    DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.3"),
                                    IncludeOption = X509IncludeOption.None
                                };
            var contentInfoForSigning = new ContentInfo(messageForSigningInBytes);
            const SubjectIdentifierType subjectIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;
            var signer = new SignedCms(subjectIdentifierType, contentInfoForSigning);
            signer.ComputeSignature(cmsSigner);
            byte[] signedMessageInBytes = signer.Encode();
            string a = StringManipulation.BytesToBase64String(signedMessageInBytes);
            return a;
        }


        /// <summary>
        ///     Uses public key to verify that message is signed with the right certificate.
        /// </summary>
        /// <param name="publicCertificate">Certificate containing public key.</param>
        /// <param name="signedMessageInBytes">Message for verifying.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns>True if massage can be varified else false.</returns>
        public static bool CmsSignedMessageVerifier(X509Certificate2 publicCertificate, string signedMessage)
        {
            byte[] signedMessageInBytes = StringManipulation.StringToBase64ByteArray(signedMessage);
            const SubjectIdentifierType subjectIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;
            var verifier = new SignedCms(subjectIdentifierType);

            try
            {
                verifier.Decode(signedMessageInBytes);
                var publicCert = new X509Certificate2Collection(publicCertificate);
                verifier.CheckSignature(publicCert, true);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public static X509Certificate2 GetCertificate(string store, string thumbprint)
        {
            var myStore = new X509Store(store, StoreLocation.CurrentUser);
            myStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2 privateCertificate =
                myStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint.ToUpper(), true)[0];
            myStore.Close();
            return privateCertificate;
        }
    }
}
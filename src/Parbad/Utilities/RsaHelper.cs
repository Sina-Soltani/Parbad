using System;
using System.Security.Cryptography;
using System.Xml;

namespace Parbad.Utilities
{
    internal static class RsaHelper
    {
        public static void FromXml(this RSACryptoServiceProvider rsa, string xmlString)
        {
            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (!xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                throw new Exception("Invalid XML RSA key.");
            }

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                    case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                    case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
                    case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
                    case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
                    case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
                    case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                    case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
                }
            }

            rsa.ImportParameters(parameters);
        }
    }
}

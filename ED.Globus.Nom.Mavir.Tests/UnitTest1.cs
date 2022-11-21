using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;

namespace ED.Globus.Nom.Mavir.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            var ackXmlPath = @"./Resources/MavirReceiveAck.xml";

            string ackXml = System.IO.File.ReadAllText(ackXmlPath, System.Text.Encoding.UTF8);
            ackXml = ackXml.Replace("\n", "").Replace("\r", "");

            var ackIdentification = GetIdentificationFromXml(ackXml, "DocumentIdentification") ??
                GetIdentificationFromXml(ackXml, "MessageIndentification");

            Assert.Equal("ACKID354554", ackIdentification);

            var anoXmlPath = @"./Resources/MavirReceiveAno.xml";

            var anoXml = System.IO.File.ReadAllText(anoXmlPath, System.Text.Encoding.UTF8);

            var anoIdentification = GetIdentificationFromXml(anoXml, "DocumentIdentification") ??
                GetIdentificationFromXml(anoXml, "MessageIdentification");

            Assert.Equal("ANO20221115131504715", anoIdentification);
        }

        private static string GetIdentificationFromXml(string ackXml, string identificationKey)
        {
            string result = null;
            var regexMatch = Regex.Match(ackXml, $"{identificationKey}.*");
            if (regexMatch.Success)
            {

                string key = regexMatch.Groups[0].Value;
                if (!string.IsNullOrEmpty(key))
                {
                    var valueRegex = Regex.Match(key, "(\".*\")/>");
                    if (valueRegex.Success)
                    {

                        result = valueRegex.Groups[0].Value;
                        result = result.Replace("\"", string.Empty);
                        var tempResult = result;
                        
                        var firstIndeOf = tempResult.IndexOf(">");
                        if(firstIndeOf == -1)
                        {
                            return null;
                        }
                        result = tempResult.Substring(0, firstIndeOf-1);
                        

                        
                    }
                }

            }

            return result;
        }
    }
}

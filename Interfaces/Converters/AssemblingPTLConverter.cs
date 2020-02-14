using DataAccess.Other;
using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace Interfaces.Converters
{
    public class AssemblingPTLConverter
    {
        public static AssemblyLightOrder ConvertRequest(string xml)
        {

            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement indicationElement = requestElement.Descendants("ASSEMBLE").First();

            AssemblyLightOrder model = new AssemblyLightOrder();
            model.MaterialId = indicationElement.Element("PRDSEQ").Value;
            return model;

        }

        public static string ConvertResponse(string xml, bool result, string message)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement responseElement = dataElement.Descendants("Response").First();
            XElement indicationElement = requestElement.Descendants("ASSEMBLE").First();

            requestElement.RemoveNodes();
            responseElement.Add(indicationElement);

            XElement TYPE = new XElement("TYPE");
            XElement MESSAGE = new XElement("MESSAGE");
            if (result)
            {
                TYPE.Value = "succ";
            }
            else
            {
                TYPE.Value = "error";
            }
            MESSAGE.Value = message;

            indicationElement.Add(TYPE);
            indicationElement.Add(MESSAGE);

            return Converter.ToXml(xDocument);
        }
    }
}

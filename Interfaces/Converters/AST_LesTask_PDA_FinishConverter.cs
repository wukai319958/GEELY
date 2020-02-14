using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Interfaces.Converters
{
    public static class AST_LesTask_PDA_FinishConverter
    {
        public static AST_LesTask_PDA_Finish ConvertRequestPDA(string xml)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();

            AST_LesTask_PDA_Finish result = new AST_LesTask_PDA_Finish();
            result.ID = arrivedElement.Element("ID").Value;
            result.BILLID = arrivedElement.Element("BILLID").Value;
            result.STATUS = int.Parse(arrivedElement.Element("STATUS").Value);
            result.BOXCODE = arrivedElement.Element("BOXCODE").Value;
            result.PALLETCODE = arrivedElement.Element("PALLETCODE").Value;
            result.QUANTITY = double.Parse(arrivedElement.Element("QUANTITY").Value);
            result.BATCH = arrivedElement.Element("BATCH").Value;

            return result;
        }
    }
}

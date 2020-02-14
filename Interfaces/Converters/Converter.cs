using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Interfaces.Converters
{
    /// <summary>
    /// 为报文转换器提供支持。
    /// </summary>
    static class Converter
    {
        /// <summary>
        /// 将 XML 文档转换成字符串。
        /// </summary>
        /// <param name="xDocument">XML 文档。</param>
        /// <returns>文档的文本表示。</returns>
        public static string ToXml(XDocument xDocument)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false)))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;

                    xDocument.WriteTo(xmlTextWriter);
                }

                byte[] memoryBytes = memoryStream.ToArray();
                return Encoding.UTF8.GetString(memoryBytes);
            }
        }
    }
}
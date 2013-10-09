using System;
using System.Xml.Linq;

namespace GameUtils
{
    public static class XmlUtils
    {
        public static T LoadXml<T>(string path, Func<XDocument, T> parser)
        {// Loading from a file, you can also load from a stream

            XDocument doc = XDocument.Load(path);

            return parser(doc);
        }
    }
}
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

    public static class BitConvertUtils
    {
        public static void WriteBytes(Int16 valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(Int16);
        }

        public static void WriteBytes(Int32 valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(Int32);
        }
        public static void WriteBytes(bool valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(bool);
        }

        public static void WriteBytes(char valueObject, byte[] bytes, ref int index)
        {
            BitConverter.GetBytes(valueObject).CopyTo(bytes, index);
            index += sizeof(char);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref char value)
        {
            value = BitConverter.ToChar(bytes, index);
            index += sizeof(char);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref bool value)
        {
            value = BitConverter.ToBoolean(bytes, index);
            index += sizeof(bool);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref Int16 value)
        {
            value = BitConverter.ToInt16(bytes, index);
            index += sizeof(Int16);
        }

        public static void ReadBytes(byte[] bytes, ref int index, ref Int32 value)
        {
            value = BitConverter.ToInt32(bytes, index);
            index += sizeof(Int32);
        }
    }
}
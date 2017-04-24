using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Texxtoor.Editor.Core.Extensions.Epub
{

    public static class Helper
    {

        public static string CreatePath(string path, string file)
        {
            return String.Format("{0}{1}{2}", path, (String.IsNullOrEmpty(path) ? "" : "/"), file);
        }

        public static string NullSaveString(XAttribute a)
        {
            if (a == null) return String.Empty;
            return a.Value;
        }
        public static string NullSaveString(XAttribute a, bool urlDecoded)
        {
            if (a == null) return String.Empty;
            return (urlDecoded) ? HttpUtility.UrlDecode(a.Value) : a.Value;
        }
        public static int NullSaveInt32(this string a)
        {
            if (String.IsNullOrEmpty(a)) return 0;
            int outValue;
            if (Int32.TryParse(a, out outValue))
            {
                return outValue;
            }
            return 0;
        }
        public static int NullSaveInt32(XAttribute a)
        {
            if (a == null) return 0;
            int outValue;
            if (Int32.TryParse(a.Value, out outValue))
            {
                return outValue;
            }
            return 0;
        }
        public static bool? NullSaveBool(XAttribute a)
        {
            if (a == null || a.Value == null) return null;
            bool outValue;
            if (Boolean.TryParse(a.Value, out outValue))
            {
                return outValue;
            }
            if (a.Value.ToLower().Equals("no")) return false;
            if (a.Value.ToLower().Equals("yes")) return true;
            return null;
        }

        /// <summary>
        /// Get the attribute name of an element's class using the specified custom attribute.
        /// </summary>
        /// <typeparam name="T">The type we're looking for</typeparam>
        /// <typeparam name="A">The decorator attribute that determines the name</typeparam>
        /// <param name="func">A resolver function for the property.</param>
        /// <returns>The XML attributes name</returns>
        public static string GetAttributeName<T, A>(Expression<Func<T, object>> func) where T : class
        {
            dynamic att = null;
            if (func.Body is MemberExpression)
            {
                att = ((MemberExpression)func.Body).Member.GetCustomAttributes(typeof(A), false).First() as Attribute;
            }
            if (func.Body is UnaryExpression)
            {
                att = ((MemberExpression)(func.Body as UnaryExpression).Operand).Member.GetCustomAttributes(typeof(A), false).First() as Attribute;
            }
            if (att != null && att is A)
            {
                return att.Name;
            }
            return null;
        }

        public static string NullSaveString(XNamespace dc, XElement e, string childName)
        {
            XElement o = ((XElement)e).Element((dc == null ? ((XElement)e).GetNamespaceOfPrefix("dc") : dc) + childName);
            if (o == null) return String.Empty;
            return o.Value;
        }

        /// <summary>
        /// Searches elements for an specific attribute and returns another attribute's value on first hit.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selectAttribute"></param>
        /// <param name="dataAttribute"></param>
        /// <returns></returns>
        public static string ReadAttributeFromElement(IEnumerable<XElement> element, string selectAttribute, string dataAttribute)
        {
            var result = element.FirstOrDefault(e => e.Attribute("name").Value == selectAttribute);
            if (result != null)
            {
                var data = result.Attribute(dataAttribute).Value;
                return data;
            }
            return String.Empty;
        }

        public static void CopyProperties<T>(T source, object target, params Expression<Func<T, object>>[] expressions)
        { // ,params string[] properties) {            
            foreach (var expression in expressions)
            {
                Expression exp = expression.Body;
                string name = null;
                if (exp.NodeType == ExpressionType.MemberAccess)
                {
                    name = ((MemberExpression)exp).Member.Name;
                }
                if (exp.NodeType == ExpressionType.Convert)
                {
                    name = ((MemberExpression)((UnaryExpression)exp).Operand).Member.Name;
                }
                if (!String.IsNullOrEmpty(name))
                {
                    PropertyInfo piSource = source.GetType().GetProperty(name);
                    PropertyInfo piTarget = target.GetType().GetProperty(name);
                    if (piTarget != null)
                    {
                        object value = piSource.GetValue(source, null);
                        piTarget.SetValue(target, value, null);
                    }
                }
            }
        }

        public static byte[] ReadStreamToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

    }
}
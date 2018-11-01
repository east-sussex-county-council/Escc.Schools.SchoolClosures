using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;

namespace Escc.Schools.SchoolClosures
{
    /// <summary>
    /// Tools to work with displaying XML in a Razor view
    /// </summary>
    public static class HtmlHelpersForXml
    {
        /// <summary>
        /// Applies an XSL transformation to an XML document.
        /// </summary>
        public static HtmlString RenderXml(this HtmlHelper helper, string xml, string xsltPath, IDictionary<string, string> parameters)
        {
            XsltArgumentList args = new XsltArgumentList();
            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    args.AddParam(key, "", parameters[key]);
                }
            }

            var t = new XslCompiledTransform();
            t.Load(xsltPath);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.ValidationType = ValidationType.None;

            using (XmlReader reader = XmlReader.Create(new StringReader(xml), settings))
            {
                var writer = new StringWriter();
                t.Transform(reader, args, writer);
                return new HtmlString(writer.ToString());
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Escc.Schools.SchoolClosures.Models
{
    /// <summary>
    /// View model for the RSS feed of school closures
    /// </summary>
    public class RssViewModel
    {
        /// <summary>
        /// Gets or sets the all closures as an XML string.
        /// </summary>
        /// <value>
        /// The closure XML.
        /// </value>
        public string ClosureXml { get; set; }

        /// <summary>
        /// Gets or sets the path and filename of the XSLT stylesheet used to build the RSS feed.
        /// </summary>
        /// <value>
        /// The stylesheet filename.
        /// </value>
        public string StylesheetFilename { get; set; }

        /// <summary>
        /// Gets the parameters to be passed to the XSLT for processing.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Dictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();
    }
}

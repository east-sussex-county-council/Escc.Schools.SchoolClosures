using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Escc.Schools.SchoolClosures
{
    public class SchoolUrl
    {
        /// <summary>
        /// Gets the URL for the profile page of a school
        /// </summary>
        /// <param name="schoolCode">The school code.</param>
        /// <returns></returns>
        public static Uri FormatSchoolUrl(string schoolCode)
        {
            return new Uri(String.Format(CultureInfo.InvariantCulture, ConfigurationManager.AppSettings["SchoolUrl"], schoolCode), UriKind.RelativeOrAbsolute);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.XPath;
using Escc.Dates;
using Escc.EastSussexGovUK.Mvc;
using Escc.Schools.SchoolClosures.Models;
using Escc.ServiceClosures;
using Escc.Web;
using Escc.Web.Metadata;
using Exceptionless;

namespace Escc.Schools.SchoolClosures.Controllers
{
    /// <summary>
    /// Controller for displaying school closure data
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class SchoolClosuresController : Controller
    {
        /// <summary>
        /// Displays the list of closures as HTML
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public async Task<ActionResult> Index(string date)
        {
            var model = new SchoolClosuresViewModel() { ShowPlannedClosures = false, ShowEmergencyClosures = true };

            // This page displays closures for a specific day. By default, that's today, but can be set to a different day.
            model.TargetDay = TargetDayForClosures(date, DateTime.Now.ToUkDateTime());

            model.Metadata.Title = BuildPageTitle(model);

            // If the date is in the past, the closures won't be listed any more so we want to return a 
            // response that'll tell search engines not to list this page
            if (model.TargetDay < DateTime.Today) new HttpStatus().Gone();

            IServiceClosureData closureData = await LoadClosureData();

            if (closureData != null && closureData.EmergencyClosureExists(model.TargetDay.Value))
            {
                // Get all schools from the XML and add them to the list.
                // List has the TargetDate set which filters the data
                var allServices = closureData.Services();
                foreach (var service in allServices)
                {
                    service.Url = PrepareAbsoluteUrl(SchoolUrl.FormatSchoolUrl(service.Code).ToString());
                    model.Services.Add(service);
                }
            }

            // Add RSS link to metadata for autodiscovery
            if (Url != null)
            {
                model.Metadata.RssFeeds.Add(new FeedUrl(new Uri(Url.Content("~/closuresrss.aspx"), UriKind.Relative), "alternate", "School closures"));
            }

            // Support the website template
            var templateRequest = new EastSussexGovUKTemplateRequest(Request);
            try
            {
                model.WebChat = await templateRequest.RequestWebChatSettingsAsync();
            }
            catch (Exception ex)
            {
                // Failure to get webchat settings should be reported, but should not cause the page to fail
                ex.ToExceptionless().Submit();
            }
            try
            {
                model.TemplateHtml = await templateRequest.RequestTemplateHtmlAsync();
            }
            catch (Exception ex)
            {
                // Failure to get the template should be reported, but should not cause the page to fail
                ex.ToExceptionless().Submit();
            }

            return View(model);
        }

        private async Task<IServiceClosureData> LoadClosureData()
        {
            IServiceClosureData closureData;
            if (HttpContext.Cache["school-closures"] != null)
            {
                closureData = HttpContext.Cache["school-closures"] as IServiceClosureData;
            }
            else
            {
                closureData = await new AzureBlobStorageDataSource(ConfigurationManager.ConnectionStrings["Escc.ServiceClosures.AzureStorage"].ConnectionString, "service-closures").ReadClosureDataAsync(new ServiceType("school"));
                HttpContext.Cache.Insert("school-closures", closureData, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
            }

            return closureData;
        }

        /// <summary>
        /// Displays a list of closures as RSS
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        //[OutputCache(Duration =300, VaryByParam ="service")]
        public async Task<ActionResult> Rss(string service)
        {
            var model = new RssViewModel();

            // Get the data on schools service closures
            var closureData = await LoadClosureData();
            var nav = (closureData as XPathDocument).CreateNavigator();
            nav.MoveToRoot();
            model.ClosureXml = nav.OuterXml;

            model.StylesheetFilename = HostingEnvironment.MapPath("~/App_Data/ClosuresRss.xslt");

            // Configure the XSLT transform.
            // Date in RFC 822 format is for whenever a date is included in RSS data
            // Date in ISO 8601 format us used to filter the closure data which has expired
            model.Parameters.Add("Rfc822Date", DateTime.Now.ToRfc822DateTime());
            model.Parameters.Add("Iso8601Date", DateTime.Now.ToIso8601DateTime());
            model.Parameters.Add("CurrentUrl", PrepareAbsoluteUrl(Request.Url.PathAndQuery).ToString());
            model.Parameters.Add("HtmlXsltUrl", PrepareAbsoluteUrl(Url.Content("~/eastsussexgovuk/rss/rss-to-html.ashx")).ToString());
            model.Parameters.Add("CssUrl", PrepareAbsoluteUrl(Url.Content("~/eastsussexgovuk/rss/rss.css")).ToString());
            model.Parameters.Add("ImageUrl", PrepareAbsoluteUrl(Url.Content("~/eastsussexgovuk/rss/escc-logo-for-feed.gif")).ToString());

            var serviceUrlStart = PrepareAbsoluteUrl(SchoolUrl.FormatSchoolUrl("{0}").ToString()) + ConfigurationManager.AppSettings["RssCampaignTracking"];
            var codeIndex = serviceUrlStart.IndexOf("{0}", StringComparison.OrdinalIgnoreCase);
            if (codeIndex > -1)
            {
                model.Parameters.Add("ServiceUrlAfterCode", serviceUrlStart.Substring(codeIndex + 3));
                serviceUrlStart = serviceUrlStart.Substring(0, codeIndex);
            }
            model.Parameters.Add("ServiceUrlBeforeCode", serviceUrlStart);

            // If the feed is for a single school, get the URL of the school's page.
            // Otherwise use the school closures page.
            if (!String.IsNullOrEmpty(Request.QueryString["service"]))
            {
                model.Parameters.Add("ServiceCode", Request.QueryString["service"]); // expects school code, for example 8454045;

                model.Parameters.Add("XhtmlVersionUrl", PrepareAbsoluteUrl(SchoolUrl.FormatSchoolUrl(service).ToString()).ToString());
            }
            else
            {
                model.Parameters.Add("XhtmlVersionUrl", PrepareAbsoluteUrl(Url.Content("~/")).ToString());
            }

            return View(model);
        }

        private Uri PrepareAbsoluteUrl(string url)
        {
            // Resolve URL if it is app-relative
            var resolvedUrl = Url.Content(url);

            // URLs need to be absolute because RSS feed can be interpreted in contexts away from the website. 
            // If not absolute already, force HTTPS in case internal reverse proxied request uses HTTP.
            var urlToReturn = new Uri(resolvedUrl, UriKind.RelativeOrAbsolute);
            if (!urlToReturn.IsAbsoluteUri)
            {
                urlToReturn = new Uri(Uri.UriSchemeHttps + "://" + HttpContext.Request.Url.Authority + resolvedUrl);
            }
            return urlToReturn;
        }

        /// <summary>
        /// Output-caches the display of a given list of closures.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [ChildActionOnly]
        [OutputCache(Duration = 300, VaryByParam = "date")]
        public PartialViewResult ClosureListCached(ISchoolClosuresViewModel model, string date)
        {
            return PartialView("~/Views/Shared/ServiceClosures/_ClosureList.cshtml", model);
        }

        [NonAction]
        public string BuildPageTitle(ISchoolClosuresViewModel model)
        {
            var title = "List of emergency school closures";
            if (!model.IsToday() && !model.IsTomorrow())
            {
                // Add date to page title
                string titleSuffix = " on " + model.TargetDay.ToBritishDateWithDay();
                title += titleSuffix;
            }
            return title;
        }

        [NonAction]
        public DateTime? TargetDayForClosures(string targetDate, DateTime today)
        {
            DateTime? targetDay = null;
            if (!String.IsNullOrEmpty(targetDate) && Regex.IsMatch(targetDate, "^[0-9]{4}-[0-9]{2}-[0-9]{2}$"))
            {
                try
                {
                    targetDay = new DateTime(Int32.Parse(targetDate.Substring(0, 4)), Int32.Parse(targetDate.Substring(5, 2)), Int32.Parse(targetDate.Substring(8, 2)));
                }
                catch (ArgumentOutOfRangeException)
                {
                    // date wasn't valid, ignore it and use the default of "today" instead
                }
            }

            if (!targetDay.HasValue)
            {
                targetDay = today.Date;
                if (today >= targetDay.Value.AddHours(15).AddMinutes(30))
                {
                    targetDay = targetDay.Value.AddDays(1);
                }

            }

            return targetDay;
        }
    }
}
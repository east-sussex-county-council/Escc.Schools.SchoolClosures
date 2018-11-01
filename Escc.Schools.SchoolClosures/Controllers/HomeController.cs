using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using Escc.Dates;
using Escc.Schools.SchoolClosures.Models;
using Escc.ServiceClosures;
using Escc.Web;
using Escc.Web.Metadata;

namespace Escc.Schools.SchoolClosures.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(string date)
        {
            var model = new SchoolClosuresViewModel() { ShowPlannedClosures = false, ShowEmergencyClosures = true, GroupByClosureStatus = true };

            // This page displays closures for a specific day. By default, that's today, but can be set to a different day.
            model.TargetDay = TargetDayForClosures(date, DateTime.Now.ToUkDateTime());

            model.Metadata.Title = BuildPageTitle(model);

            // If the date is in the past, the closures won't be listed any more so we want to return a 
            // response that'll tell search engines not to list this page
            if (model.TargetDay < DateTime.Today) new HttpStatus().Gone();

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

            if (closureData != null && closureData.EmergencyClosureExists(model.TargetDay.Value))
            {
                // Get all schools from the XML and add them to the list.
                // List has the TargetDate set which filters the data
                var allServices = closureData.Services();
                foreach (var service in allServices)
                {
                    service.Url = SchoolUrl.FormatSchoolUrl(service.Code, Request.Url);
                    model.Services.Add(service);
                }
            }

            // Add RSS link to metadata for autodiscovery
            if (Url != null)
            {
                model.Metadata.RssFeeds.Add(new FeedUrl(new Uri(Url.Content("~/closuresrss.aspx"), UriKind.Relative), "alternate", "School closures"));
            }
            
            return View(model);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 300, VaryByParam = "date")]
        public PartialViewResult ClosureListCached(SchoolClosuresViewModel model, string date)
        {
            return PartialView("_ClosureList", model);
        }

        [NonAction]
        public string BuildPageTitle(SchoolClosuresViewModel model)
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
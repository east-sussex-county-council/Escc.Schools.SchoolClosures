using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Escc.Schools.SchoolClosures.Controllers;
using Escc.Schools.SchoolClosures.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Escc.Schools.SchoolClosures.Tests
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void TodayCreatesTitleWithoutDate()
        {
            var controller = new HomeController();
            var today = new DateTime(2018, 10, 29);

            var result = controller.BuildPageTitle(new SchoolClosuresViewModel(today) { TargetDay = today });

            Assert.AreEqual("List of emergency school closures", result);
        }

        [TestMethod]
        public void TomorrowCreatesTitleWithoutDate()
        {
            var controller = new HomeController();
            var today = new DateTime(2018, 10, 29);

            var result = controller.BuildPageTitle(new SchoolClosuresViewModel(today) { TargetDay = today.AddDays(1) });

            Assert.AreEqual("List of emergency school closures", result);
        }

        [TestMethod]
        public void FutureDateCreatesTitleWithDate()
        {
            var controller = new HomeController();
            var today = new DateTime(2018, 10, 29);

            var result = controller.BuildPageTitle(new SchoolClosuresViewModel(today) { TargetDay = today.AddDays(2) });

            Assert.AreEqual("List of emergency school closures on Wednesday 31 October 2018", result);
        }

        [TestMethod]
        public void TargetDateInIso8601FormatIsRead()
        {
            var controller = new HomeController();

            var result = controller.TargetDayForClosures("2018-11-29", new DateTime(2018, 11, 29));

            Assert.AreEqual(new DateTime(2018, 11, 29), result);
        }

        [TestMethod]
        public void InvalidDateReturnsToday()
        {
            var controller = new HomeController();

            var result = controller.TargetDayForClosures("2018-11-31", new DateTime(2018, 11, 29));

            Assert.AreEqual(new DateTime(2018, 11, 29), result);
        }

        [TestMethod]
        public void BlankDateReturnsToday()
        {
            var controller = new HomeController();

            var result = controller.TargetDayForClosures(String.Empty, new DateTime(2018, 11, 29));

            Assert.AreEqual(new DateTime(2018, 11, 29), result);
        }

        [TestMethod]
        public void BlankDateReturnsTomorrowAfter330pm()
        {
            var controller = new HomeController();

            var result = controller.TargetDayForClosures(String.Empty, new DateTime(2018, 11, 29, 15, 30, 0));

            Assert.AreEqual(new DateTime(2018, 11, 30), result);
        }
    }
}

using System;
using Escc.Schools.SchoolClosures.Controllers;
using Escc.Schools.SchoolClosures.Models;
using Moq;
using NUnit.Framework;

namespace Escc.Schools.SchoolClosures.Tests
{
    [TestFixture]
    public class SchoolClosuresControllerTests
    {
        [Test]
        public void TodayCreatesTitleWithoutDate()
        {
            var controller = new SchoolClosuresController();
            var model = new Mock<ISchoolClosuresViewModel>();
            model.Setup(x => x.IsToday()).Returns(true);
            model.Setup(x => x.IsTomorrow()).Returns(false);
            model.Setup(x => x.TargetDay).Returns(new DateTime(2018, 10, 29));

            var result = controller.BuildPageTitle(model.Object);

            Assert.AreEqual("List of emergency school closures", result);
        }

        [Test]
        public void TomorrowCreatesTitleWithoutDate()
        {
            var controller = new SchoolClosuresController();
            var model = new Mock<ISchoolClosuresViewModel>();
            model.Setup(x => x.IsToday()).Returns(false);
            model.Setup(x => x.IsTomorrow()).Returns(true);
            model.Setup(x => x.TargetDay).Returns(new DateTime(2018, 10, 29).AddDays(1));

            var result = controller.BuildPageTitle(model.Object);

            Assert.AreEqual("List of emergency school closures", result);
        }

        [Test]
        public void FutureDateCreatesTitleWithDate()
        {
            var controller = new SchoolClosuresController();
            var model = new Mock<ISchoolClosuresViewModel>();
            model.Setup(x => x.IsToday()).Returns(false);
            model.Setup(x => x.IsTomorrow()).Returns(false);
            model.Setup(x => x.TargetDay).Returns(new DateTime(2018, 10, 29).AddDays(2));

            var result = controller.BuildPageTitle(model.Object);

            Assert.AreEqual("List of emergency school closures on Wednesday 31 October 2018", result);
        }

        [Test]
        public void TargetDateInIso8601FormatIsRead()
        {
            var controller = new SchoolClosuresController();

            var result = controller.TargetDayForClosures("2018-11-29", new DateTime(2018, 11, 29));

            Assert.AreEqual(new DateTime(2018, 11, 29), result);
        }

        [Test]
        public void InvalidDateReturnsToday()
        {
            var controller = new SchoolClosuresController();

            var result = controller.TargetDayForClosures("2018-11-31", new DateTime(2018, 11, 29));

            Assert.AreEqual(new DateTime(2018, 11, 29), result);
        }

        [Test]
        public void BlankDateReturnsToday()
        {
            var controller = new SchoolClosuresController();

            var result = controller.TargetDayForClosures(String.Empty, new DateTime(2018, 11, 29));

            Assert.AreEqual(new DateTime(2018, 11, 29), result);
        }

        [Test]
        public void BlankDateReturnsTomorrowAfter330pm()
        {
            var controller = new SchoolClosuresController();

            var result = controller.TargetDayForClosures(String.Empty, new DateTime(2018, 11, 29, 15, 30, 0));

            Assert.AreEqual(new DateTime(2018, 11, 30), result);
        }
    }
}

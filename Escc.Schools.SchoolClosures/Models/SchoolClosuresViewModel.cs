using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Escc.Dates;
using Escc.EastSussexGovUK.Mvc;
using Escc.ServiceClosures;

namespace Escc.Schools.SchoolClosures.Models
{
    public class SchoolClosuresViewModel : BaseViewModel
    {
        private DateTime _today;

        public SchoolClosuresViewModel() { _today = DateTime.Now.ToUkDateTime().Date;  }

        public SchoolClosuresViewModel(DateTime today) { _today = today; }

        public bool IsToday() { return TargetDay.HasValue && TargetDay.Value == _today; }

        public bool IsTomorrow() { return TargetDay.HasValue && TargetDay.Value == _today.AddDays(1); }

        public DateTime? TargetDay { get; set; }

        /// <summary>
        /// Gets the services containing the closure data.
        /// </summary>
        /// <value>The closures.</value>
        public Collection<Service> Services { get; } = new Collection<Service>();

        /// <summary>
        /// Gets or sets a value indicating whether to show only closures in the future.
        /// </summary>
        /// <value><c>true</c> to show future closures only; otherwise, <c>false</c>.</value>
        public bool FutureOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show emergency closures.
        /// </summary>
        /// <value><c>true</c> to show emergency closures; otherwise, <c>false</c>.</value>
        public bool ShowEmergencyClosures { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show closures which are planned in advance and are not emergencies.
        /// </summary>
        /// <value><c>true</c> to show planned closures; otherwise, <c>false</c>.</value>
        public bool ShowPlannedClosures { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to group by closure status.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if grouping by closure status; otherwise, <c>false</c>.
        /// </value>
        public bool GroupByClosureStatus { get; set; }
    }
}
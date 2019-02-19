using System;
using System.Collections.ObjectModel;
using Escc.ServiceClosures;
using Escc.ServiceClosures.Mvc;

namespace Escc.Schools.SchoolClosures.Models
{
     /// <summary>
    /// View model to display a list of school closures within the East Sussex County Council website template
    /// </summary>
    public interface ISchoolClosuresViewModel : IServiceClosureListViewModel
    {
    }
}
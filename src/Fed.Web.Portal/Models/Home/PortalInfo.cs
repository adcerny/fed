using Fed.Core.Entities;
using System.Diagnostics;

namespace Fed.Web.Portal.Models.Home
{
    public static class PortalInfo
    {
        public static AppInfo GetInfo()
        {
            var assembly = typeof(Startup).Assembly;
            var creationDate = System.IO.File.GetCreationTime(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            return new AppInfo { Version = version, LastPublished = creationDate };
        }
    }
}
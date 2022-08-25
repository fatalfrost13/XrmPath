using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.Web.Controllers
{
    [Route("TestController/[action]")]
    public class TestController : Controller
    {
        private readonly UmbracoHelper _umbracoHelper;
        private readonly AppSettingsModel _appSettings;
        public TestController(UmbracoHelper umbracoHelper, IOptions<AppSettingsModel> appSettings) { 
            _umbracoHelper = umbracoHelper;
            _appSettings = appSettings.Value;
        }

        public IActionResult GetHomeNodeName()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();

            if (rootNode is null)
            {
                return NotFound();
            }

            return Ok(rootNode.Name);
        }

        public string AppSettings() {
            string appValue = "";
            if (_appSettings != null) {
                appValue = _appSettings.DateFormat ?? "";
            }
            return appValue;
        }
    }
}
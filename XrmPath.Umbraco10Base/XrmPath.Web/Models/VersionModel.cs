using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XrmPath.UmbracoCore.Models
{
    public static class VersionModel
    {
        private static long _JsVersionNumber = 0; // field
        public static long JsVersionNumber
        {
            get
            {
                if (_JsVersionNumber != 0)
                {
                    return _JsVersionNumber;
                }
                var versionNumber = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                _JsVersionNumber = versionNumber;
                return _JsVersionNumber;
            }
            set
            {
                _JsVersionNumber = value;
            }
        }
    }
}
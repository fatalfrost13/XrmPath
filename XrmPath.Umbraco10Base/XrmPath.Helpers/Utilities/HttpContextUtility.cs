using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.Helpers.Utilities
{
    public static class HttpHelper
    {
        private static IHttpContextAccessor? _accessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        public static HttpContext? HttpContext => _accessor?.HttpContext;
    }
}

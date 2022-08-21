using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace XrmPath.Web.Models
{
    public class ColorPickerModel
    {
        [JsonProperty("label")]
        public string ColorLabel { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string ColorValue { get; set; } = string.Empty;
    }
}
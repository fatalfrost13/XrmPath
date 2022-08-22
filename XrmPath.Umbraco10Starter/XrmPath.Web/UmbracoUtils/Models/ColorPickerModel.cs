using Newtonsoft.Json;

namespace XrmPath.UmbracoUtils.Models
{
    public class ColorPickerModel
    {
        [JsonProperty("label")]
        public string ColorLabel { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string ColorValue { get; set; } = string.Empty;
    }
}
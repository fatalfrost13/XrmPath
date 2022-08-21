using Examine;
using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core.Models;

namespace XrmPath.Web.Models
{
    public class MediaValues
    {
        public MediaValues(XPathNavigator xpath)
        {
            int mediaId;
            int.TryParse(xpath?.GetAttribute("id", ""), out mediaId);

            //if (xpath == null) throw new ArgumentNullException(nameof(xpath));
            Id = mediaId;
            Name = xpath?.GetAttribute("nodeName", "") ?? string.Empty;
            Values = new Dictionary<string, string>();
            var result = xpath?.SelectChildren(XPathNodeType.Element) ?? null;
            if (result != null)
            {
                while (result.MoveNext())
                {
                    if (result.Current != null && !result.Current.HasAttributes)
                    {
                        Values.Add(result.Current.Name, result.Current.Value);
                    }
                }
            }
        }

        public MediaValues(SearchResult result)
        {
            //if (result == null) throw new ArgumentNullException(nameof(result));
            Id = result?.Id ?? 0;
            Alias = result?.Fields["nodeTypeAlias"] ?? string.Empty;
            Name = result?.Fields["nodeName"] ?? string.Empty;
            Values = result?.Fields ?? new Dictionary<string, string>();
        }

        public MediaValues(IPublishedContent mediaContent)
        {
            //if (mediaContent == null) throw new ArgumentNullException(nameof(mediaContent));
            Id = mediaContent?.Id ?? 0;
            Alias = mediaContent?.DocumentTypeAlias ?? string.Empty;
            Name = mediaContent?.Name ?? string.Empty;
            //Values = new Dictionary<string, string>() { {"umbracoFile" , mediaContent.Url } };
            Values = new Dictionary<string, string>();
            if (mediaContent != null)
            {
                foreach (var property in mediaContent.Properties)
                {
                    Values.Add(property.PropertyTypeAlias, property.Value.ToString());
                }
            }

        }

        public int Id { get; private set; }
        public string Alias { get; private set; }
        public string Name { get; private set; }
        public IDictionary<string, string> Values { get; private set; }
        public DateTime MediaDateTime { get; set; } = DateTime.UtcNow;

    }

    public class MediaValuesModel
    {
        public List<MediaValues> MediaList { get; set; } = new List<MediaValues>();
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    }
}
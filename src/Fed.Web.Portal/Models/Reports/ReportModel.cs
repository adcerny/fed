using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Fed.Web.Portal.Models.Reports
{
    public class ReportModel
    {
        public class FieldLink
        {
            public FieldLink(
                string columnName,
                string sourceName,
                string placeholder,
                string relativeUrl)
            {
                ColumnName = columnName;
                SourceName = sourceName;
                Placeholder = placeholder;
                RelativeUrl = relativeUrl;
            }

            public string ColumnName { get; set; }
            public string SourceName { get; set; }
            public string Placeholder { get; set; }
            public string RelativeUrl { get; set; }
        }

        public ReportModel(
            string nameAndQuery,
            IList<JObject> rows,
            IList<FieldLink> links,
            IList<string> hiddenColumns)
        {
            NameAndQuery = nameAndQuery;
            Rows = rows;
            Links = links;
            HiddenColumns = hiddenColumns;

            ParameterValues = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(nameAndQuery))
                return;

            var parts = nameAndQuery.Split(new[] { '?' }, 2);

            if (parts.Length != 2)
                return;

            var query = parts[1];

            var queryArgs = query.Split(new[] { '&' });

            foreach (var arg in queryArgs)
            {
                var keyValue = arg.Split(new[] { '=' }, 2);

                if (keyValue.Length != 2)
                    continue;

                var key = keyValue[0];
                var value = keyValue[1];
                ParameterValues.Add(key, value);
            }
        }

        public string NameAndQuery { get; }
        public IList<JObject> Rows { get; }
        public IList<FieldLink> Links { get; }
        public IList<string> HiddenColumns { get; }
        public IDictionary<string, string> ParameterValues { get; }
        public bool DisplayInputFieldsForParameters { get; set; }
    }
}
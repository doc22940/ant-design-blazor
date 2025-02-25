﻿using System;
using System.Collections.Generic;

namespace AntBlazor.Docs.Routing
{
    public class Route
    {
        public string[] UriSegments { get; set; }
        public Type PageType { get; set; }

        internal RouteTemplate Template { get; set; }

        public string[] UnusedRouteParameterNames { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public MatchResult Match(string[] segments, string relativeUri)
        {
            var parameters = ParseQueryString(relativeUri);

            if (Template != null)
            {
                if (Template.Segments.Length != segments.Length)
                {
                    return MatchResult.NoMatch();
                }

                for (var i = 0; i < Template.Segments.Length; i++)
                {
                    var segment = Template.Segments[i];
                    var pathSegment = segments[i];
                    if (!segment.Match(pathSegment, out var matchedParameterValue))
                    {
                        return MatchResult.NoMatch();
                    }
                    else
                    {
                        if (segment.IsParameter)
                        {
                            parameters ??= new Dictionary<string, object>(StringComparer.Ordinal);
                            parameters[segment.Value] = matchedParameterValue;
                        }
                    }
                }

                // In addition to extracting parameter values from the URL, each route entry
                // also knows which other parameters should be supplied with null values. These
                // are parameters supplied by other route entries matching the same handler.
                if (UnusedRouteParameterNames.Length > 0)
                {
                    parameters ??= new Dictionary<string, object>(StringComparer.Ordinal);
                    foreach (var name in UnusedRouteParameterNames)
                    {
                        parameters[name] = null;
                    }
                }

                this.Parameters = parameters;

                return MatchResult.Match(this);
            }
            else
            {
                if (segments.Length != UriSegments.Length)
                {
                    return MatchResult.NoMatch();
                }

                for (var i = 0; i < UriSegments.Length; i++)
                {
                    if (string.Compare(segments[i], UriSegments[i], StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return MatchResult.NoMatch();
                    }
                }
                this.Parameters ??= new Dictionary<string, object>(StringComparer.Ordinal);
                return MatchResult.Match(this);
            }
        }

        private Dictionary<string, object> ParseQueryString(string uri)
        {
            // Parameters will be lazily initialized.
            Dictionary<string, object> querystring = null;

            foreach (string kvp in uri.Substring(uri.IndexOf("?", StringComparison.Ordinal) + 1).Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (kvp != "" && kvp.Contains("="))
                {
                    var pair = kvp.Split('=');
                    querystring ??= new Dictionary<string, object>(StringComparer.Ordinal);
                    querystring.Add(pair[0], pair[1]);
                }
            }

            return querystring;
        }
    }
}
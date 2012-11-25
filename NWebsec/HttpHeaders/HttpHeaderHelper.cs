﻿#region License
/*
Copyright (c) 2012, André N. Klingsheim
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Linq;
using NWebsec.Modules.Configuration;
using NWebsec.Modules.Configuration.Csp;

namespace NWebsec.HttpHeaders
{
    public class HttpHeaderHelper
    {
        private readonly string suppressHeadersKey = "NWebsecSuppressHeaders";
        private readonly string setNoCacheHeadersKey = "NWebsecSetNoCacheHeaders";
        private readonly string cspHeadersKeyPrefix = "NWebsecCsp";

        private HttpContextBase context;
        private HttpHeaderConfigurationSection baseConfig;

        public HttpHeaderHelper(HttpContextBase context)
        {
            this.context = context;
            baseConfig = GetConfig();
        }

        internal HttpHeaderHelper(HttpContextBase context, HttpHeaderConfigurationSection config)
        {
            this.context = context;
            baseConfig = config;
        }

        internal void FixHeaders()
        {
            var headerSetter = new HttpHeaderSetter(context);

            headerSetter.SuppressVersionHeaders(GetSuppressVersionHeadersWithOverride());
            headerSetter.AddHstsHeader(GetHstsWithOverride());
            headerSetter.SetNoCacheHeaders(GetNoCacheHeadersWithOverride());
            headerSetter.AddXFrameoptionsHeader(GetXFrameoptionsWithOverride());
            headerSetter.AddXContentTypeOptionsHeader(GetXContentTypeOptionsWithOverride());
            headerSetter.AddXDownloadOptionsHeader(GetXDownloadOptionsWithOverride());
            headerSetter.AddXXssProtectionHeader(GetXXssProtectionWithOverride());
            headerSetter.AddXCspHeaders(GetCspElementWithOverrides(false, baseConfig.SecurityHttpHeaders.Csp), false);
            headerSetter.AddXCspHeaders(GetCspElementWithOverrides(true, baseConfig.SecurityHttpHeaders.CspReportOnly), true);

        }

        public void SetNoCacheHeadersOverride(SimpleBooleanConfigurationElement setNoCacheHeadersConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = setNoCacheHeadersKey;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, setNoCacheHeadersConfig);
        }

        internal SimpleBooleanConfigurationElement GetNoCacheHeadersWithOverride()
        {
            var headerList = GetHeaderListFromContext();
            return headerList.ContainsKey(setNoCacheHeadersKey)
                       ? (SimpleBooleanConfigurationElement)headerList[setNoCacheHeadersKey]
                       : baseConfig.NoCacheHttpHeaders;
        }

        public void SetXFrameoptionsOverride(XFrameOptionsConfigurationElement xFrameOptionsConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = HttpHeadersConstants.XFrameOptionsHeader;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, xFrameOptionsConfig);
        }

        internal XFrameOptionsConfigurationElement GetXFrameoptionsWithOverride()
        {
            var headerList = GetHeaderListFromContext();
            return headerList.ContainsKey(HttpHeadersConstants.XFrameOptionsHeader)
                       ? (XFrameOptionsConfigurationElement)headerList[HttpHeadersConstants.XFrameOptionsHeader]
                       : baseConfig.SecurityHttpHeaders.XFrameOptions;
        }

        public void SetHstsOverride(HstsConfigurationElement hstsConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = HttpHeadersConstants.StrictTransportSecurityHeader;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, hstsConfig);
        }

        internal HstsConfigurationElement GetHstsWithOverride()
        {
            var headerList = GetHeaderListFromContext();
            return headerList.ContainsKey(HttpHeadersConstants.StrictTransportSecurityHeader)
                       ? (HstsConfigurationElement)headerList[HttpHeadersConstants.StrictTransportSecurityHeader]
                       : baseConfig.SecurityHttpHeaders.Hsts;
        }

        public void SetXContentTypeOptionsOverride(SimpleBooleanConfigurationElement xContentTypeOptionsConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = HttpHeadersConstants.XContentTypeOptionsHeader;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, xContentTypeOptionsConfig);
        }

        public SimpleBooleanConfigurationElement GetXContentTypeOptionsWithOverride()
        {
            var headerList = GetHeaderListFromContext();
            return headerList.ContainsKey(HttpHeadersConstants.XContentTypeOptionsHeader) ?
                (SimpleBooleanConfigurationElement)headerList[HttpHeadersConstants.XContentTypeOptionsHeader]
                : baseConfig.SecurityHttpHeaders.XContentTypeOptions;
        }

        public void SetXDownloadOptionsOverride(SimpleBooleanConfigurationElement xDownloadOptionsConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = HttpHeadersConstants.XDownloadOptionsHeader;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, xDownloadOptionsConfig);
        }

        internal SimpleBooleanConfigurationElement GetXDownloadOptionsWithOverride()
        {
            var headerList = GetHeaderListFromContext();
            return headerList.ContainsKey(HttpHeadersConstants.XDownloadOptionsHeader)
                    ? (SimpleBooleanConfigurationElement)headerList[HttpHeadersConstants.XDownloadOptionsHeader]
                    : baseConfig.SecurityHttpHeaders.XDownloadOptions;
        }

        public void SetXXssProtectionOverride(XXssProtectionConfigurationElement xXssProtectionConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = HttpHeadersConstants.XXssProtectionHeader;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, xXssProtectionConfig);
        }

        internal XXssProtectionConfigurationElement GetXXssProtectionWithOverride()
        {
            var headerList = GetHeaderListFromContext();

            return headerList.ContainsKey(HttpHeadersConstants.XXssProtectionHeader)
                ? (XXssProtectionConfigurationElement)headerList[HttpHeadersConstants.XXssProtectionHeader]
                : baseConfig.SecurityHttpHeaders.XXssProtection;
        }

        public void SetSuppressVersionHeadersOverride(SuppressVersionHeadersConfigurationElement suppressVersionHeadersConfig)
        {
            var headerList = GetHeaderListFromContext();
            var headerKey = suppressHeadersKey;

            if (headerList.ContainsKey(headerKey))
                headerList.Remove(headerKey);

            headerList.Add(headerKey, suppressVersionHeadersConfig);
        }

        internal SuppressVersionHeadersConfigurationElement GetSuppressVersionHeadersWithOverride()
        {
            var headerList = GetHeaderListFromContext();

            return headerList.ContainsKey(suppressHeadersKey)
                    ? (SuppressVersionHeadersConfigurationElement)headerList[suppressHeadersKey]
                    : baseConfig.SuppressVersionHeaders;
        }

        public void SetContentSecurityPolicyDirectiveOverride(string directive, string sources, bool reportOnly)
        {

            if (sources.StartsWith(" ") || sources.EndsWith(" "))
                throw new ApplicationException("Sources must not contain leading or trailing whitespace: " + sources);
            if (sources.Contains("  "))
                throw new ApplicationException("Sources must be separated by exactly one whitespace: " + sources);

            var sourceList = sources.Split(' ');

            var element = new CspDirectiveBaseConfigurationElement() { Name = directive };

            foreach (var source in sourceList)
            {
                element.Sources.Add(new CspSourceConfigurationElement() { Source = source });
            }

            var cspOverride = GetCspDirectiveOverides(reportOnly);
            if (cspOverride.ContainsKey(element.Name))
                cspOverride.Remove(element.Name);
            cspOverride.Add(element.Name, element);
        }

        private IDictionary<String, CspDirectiveBaseConfigurationElement> GetCspDirectiveOverides(bool reportOnly)
        {
            var headerKey = cspHeadersKeyPrefix + (reportOnly
                                 ? HttpHeadersConstants.XContentSecurityPolicyReportOnlyHeader
                                 : HttpHeadersConstants.XContentSecurityPolicyHeader);

            var headerList = GetHeaderListFromContext();

            if (!headerList.ContainsKey(headerKey))
                headerList[headerKey] = new Dictionary<String, CspDirectiveBaseConfigurationElement>();
            return (IDictionary<String, CspDirectiveBaseConfigurationElement>)headerList[headerKey];
        }

        internal CspConfigurationElement GetCspElementWithOverrides(bool reportOnlyElement, CspConfigurationElement cspConfig)
        {
            var headerKey = cspHeadersKeyPrefix + (reportOnlyElement
                                 ? HttpHeadersConstants.XContentSecurityPolicyReportOnlyHeader
                                 : HttpHeadersConstants.XContentSecurityPolicyHeader);

            var headerList = GetHeaderListFromContext();
            if (!headerList.ContainsKey(headerKey))
                return cspConfig;

            var overriddenConfig = new CspConfigurationElement();
            var overrides = (IDictionary<String, CspDirectiveBaseConfigurationElement>)headerList[headerKey];

            foreach (CspDirectiveBaseConfigurationElement directive in cspConfig.Directives)
            {
                if (!overrides.ContainsKey(directive.Name))
                    overriddenConfig.Directives.Add(directive);
            }


            foreach (KeyValuePair<String, CspDirectiveBaseConfigurationElement> directive in overrides)
            {
                var test = (from CspSourceConfigurationElement source in directive.Value.Sources
                            where (!String.IsNullOrEmpty(source.Source))
                            select source).ToArray();

                if (String.IsNullOrEmpty(directive.Value.Source) &&
                    test.Length == 0)
                {
                    continue;
                }

                overriddenConfig.Directives.Add(directive.Value);
            }

            if (!(overriddenConfig.XContentSecurityPolicyHeader || overriddenConfig.XWebKitCspHeader))
                overriddenConfig.XContentSecurityPolicyHeader = overriddenConfig.XWebKitCspHeader = true;

            return overriddenConfig;
        }

        private IDictionary<string, object> GetHeaderListFromContext()
        {
            if (context.Items["nwebsecheaderoverride"] == null)
            {
                context.Items["nwebsecheaderoverride"] = new Dictionary<string, object>();
            }
            return (IDictionary<string, object>)context.Items["nwebsecheaderoverride"];
        }

        public static HttpHeaderConfigurationSection GetConfig()
        {
            return (HttpHeaderConfigurationSection)(ConfigurationManager.GetSection("nwebsec/httpHeaderModule")) ??
             new HttpHeaderConfigurationSection();
        }

    }
}

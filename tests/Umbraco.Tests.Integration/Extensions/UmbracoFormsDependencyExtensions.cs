using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Forms.Core.Extensions;
using Umbraco.Forms.Core.Providers.Extensions;
using Umbraco.Forms.Web.Extensions;

namespace Umbraco.Cms.Tests.Integration.Extensions
{
    public static class UmbracoFormsDependencyExtensions
    {
        public static IUmbracoBuilder AddUmbracoFormsSupport(this IUmbracoBuilder builder)
        {
            builder.AddUmbracoFormsCore();
            builder.AddUmbracoFormsCoreProviders();
            builder.AddUmbracoFormsWeb();

            return builder;
        }
    }
}

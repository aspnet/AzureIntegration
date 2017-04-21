// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.ApplicationInsights.HostingStartup
{
    /// <summary>
    /// The <see cref="TagHelperComponent"/> that injects the <see cref="JavaScriptSnippet"/> to the head tag.
    /// </summary>
    public class JavaScriptSnippetTagHelperComponent : TagHelperComponent
    {
        private string _javaScriptSnippet;

        /// <summary>
        /// Initializes the <see cref="JavaScriptSnippetTagHelperComponent"/>.
        /// </summary>
        /// <param name="javaScriptSnippet">The <see cref="JavaScriptSnippet"/> to inject in the head tag.</param>
        public JavaScriptSnippetTagHelperComponent(JavaScriptSnippet javaScriptSnippet)
        {
            _javaScriptSnippet = javaScriptSnippet.FullScript;
        }

        /// <inheritdoc />
        public override int Order => 1;

        /// <summary>
        /// Appends the <see cref="JavaScriptSnippet"/> to the head tag.
        /// </summary>
        /// <param name="context">The <see cref="TagHelperContext"/> associated with the head tag.</param>
        /// <param name="output">The <see cref="TagHelperOutput"/> of the head tag.</param>
        /// <returns></returns>
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml(_javaScriptSnippet);
            }

            return Task.CompletedTask;
        }
    }
}

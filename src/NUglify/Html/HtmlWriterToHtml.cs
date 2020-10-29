﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NUglify.Html
{
    public class HtmlWriterToHtml : HtmlWriterBase
    {
        private readonly HtmlSettings settings;

        private bool lastNewLine = false;

        private bool allowIndent = true;

        private static readonly char[] AttributeCharsForcingQuote = new[]
        {' ', '\t', '\n', '\f', '\r', '"', '\'', '`', '=', '<', '>'};

        public HtmlWriterToHtml(TextWriter writer, HtmlSettings settings = null)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            Writer = writer;
            writer.NewLine = "\n";
            this.settings = settings;
        }


        public TextWriter Writer { get; }

        void WriteIndent()
        {
	        for (int i = 0; i < Depth; i++)
		        Writer.Write(settings.Indent);
        }

        protected override void Write(string text)
        {
            if (settings.PrettyPrint && lastNewLine && allowIndent)
            {
                this.WriteIndent();
                lastNewLine = false;
            }

            Writer.Write(text);
        }

        protected override void Write(char c)
        {
            if (settings.PrettyPrint && lastNewLine && allowIndent)
            {
	            this.WriteIndent();
                lastNewLine = false;
            }

            Writer.Write(c);
        }

        protected override void WriteStartTag(HtmlElement node)
        {
            var shouldPretty = ShouldPretty(node);
            allowIndent = (settings.PrettyPrint && !settings.TagsWithNonCollapsibleWhitespaces.ContainsKey(node.Name));
            if (shouldPretty && !lastNewLine)
            {
                Writer.WriteLine();
                lastNewLine = true;
            }

            Write("<");
            var isProcessing = (node.Kind & ElementKind.ProcessingInstruction) != 0;
            if (isProcessing) 
	            Write("?");

            Write(node.Name);

            if (node.Attributes != null)
            {
                IEnumerable<HtmlAttribute> attributes = node.Attributes;

                if (settings.AlphabeticallyOrderAttributes)
                    attributes = attributes.OrderBy(a => a.Name);

                var count = node.Attributes.Count;
                var i = 0;
                foreach (var attribute in attributes)
                {
                    Write(" ");
                    WriteAttribute(node, attribute, i++ == count);
                }
            }

            if (isProcessing) 
	            Write("?");

            if ((node.Kind & ElementKind.SelfClosing) != 0 && (XmlNamespaceLevel > 0 || !settings.RemoveOptionalTags)) 
	            Write(" /");

            Write(">");

            if (shouldPretty)
            {
                Writer.WriteLine();
                lastNewLine = true;
            }
        }

        protected override void WriteEndTag(HtmlElement node)
        {
            var shouldPretty = ShouldPretty(node);

            if (shouldPretty && !lastNewLine)
            {
                Writer.WriteLine();
                lastNewLine = true;
            }

            base.WriteEndTag(node);

            allowIndent = true;

            if (shouldPretty)
            {
                Writer.WriteLine();
                lastNewLine = true;
            }
        }

        bool ShouldPretty(HtmlElement node)
        {
            return settings.PrettyPrint && (node.Descriptor == null || !settings.InlineTagsPreservingSpacesAround.ContainsKey(node.Descriptor.Name));
        }

        protected override void WriteAttributeValue(HtmlElement element, HtmlAttribute attribute, bool isLast)
        {
            var attrValue = attribute.Value;

            char quoteChar;

            if (settings.AttributeQuoteChar == null)
            {
                var quoteCount = 0;
                var doubleQuoteCount = 0;

                for (int i = 0; i < attrValue.Length; i++)
                {
                    var c = attrValue[i];
                    if (c == '\'')
                    {
                        quoteCount++;
                    }
                    else if (c == '"')
                    {
                        doubleQuoteCount++;
                    }

                    // We also count escapes so that we have an exact count for both
                    if (c == '&')
                    {
                        if (attrValue.IndexOf("&#34;", i, StringComparison.OrdinalIgnoreCase) > 0
                            || attrValue.IndexOf("&quot;", i, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            doubleQuoteCount++;
                        }
                        else if (attrValue.IndexOf("&#39;", i, StringComparison.OrdinalIgnoreCase) > 0
                                 || attrValue.IndexOf("&apos;", i, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            quoteCount++;
                        }
                    }
                }

                quoteChar = quoteCount < doubleQuoteCount ? '\'' : '"';
            }
            else
            {
                quoteChar = settings.AttributeQuoteChar.Value == '"' ? '"' : '\'';
            }

            if (quoteChar == '"')
            {
                attrValue = attrValue.Replace("&#39;", "'");
                attrValue = attrValue.Replace("&apos;", "'");
                attrValue = attrValue.Replace("\"", "&#34;");
            }
            else
            {
                attrValue = attrValue.Replace("&#34;", "\"");
                attrValue = attrValue.Replace("&quot;", "\"");
                attrValue = attrValue.Replace("'", "&#39;");
            }

            var canRemoveQuotes = settings.RemoveQuotedAttributes && attrValue != string.Empty && attrValue.IndexOfAny(AttributeCharsForcingQuote) < 0;

            if (!canRemoveQuotes)
            {
                Write(quoteChar);
            }

            Write(attrValue);

            if (!canRemoveQuotes)
            {
                Write(quoteChar);
            }
            else
            {
                bool emitSpace = false;

                if (isLast)
                {
                    if (attrValue.EndsWith("/"))
                    {
                        emitSpace = true;
                    }
                }

                if (emitSpace)
                {
                    Write(" ");
                }
            }
        }

        protected override void Write(HtmlRaw node)
        {
	        if (ShouldPretty(node.Parent) && (node.Parent?.Name == "script" || node.Parent?.Name == "style"))
	        {
		        var lines = node.Slice.ToString().Split(new [] { Writer.NewLine }, StringSplitOptions.None);
		        // we're already indented for the first line
		        Write(lines[0]);

                for (var i = 1; i < lines.Length; i++)
                {
	                Write(Writer.NewLine);
                    WriteIndent();
	                Write(lines[i]);
                }
            }
            else
				base.Write(node);
        }
    }
}
﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
    {
        private static readonly ObjectPool<StringBuilder> Pool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public static Dictionary<string, string> ToTexts(this NamedContentData data)
        {
            var result = new Dictionary<string, string>();

            if (data != null)
            {
                var languages = new Dictionary<string, StringBuilder>();

                foreach (var value in data.Values)
                {
                    if (value != null)
                    {
                        foreach (var (key, jsonValue) in value)
                        {
                            AppendJsonText(languages, key, jsonValue);
                        }
                    }
                }

                foreach (var (key, stringBuilder) in languages)
                {
                    result[key] = stringBuilder.ToString();

                    Pool.Return(stringBuilder);
                }
            }

            return result;
        }

        private static void AppendJsonText(Dictionary<string, StringBuilder> languages, string language, IJsonValue value)
        {
            if (value.Type == JsonValueType.String)
            {
                AppendText(languages, language, value.ToString());
            }
            else if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    AppendJsonText(languages, language, item);
                }
            }
            else if (value is JsonObject obj)
            {
                foreach (var item in obj.Values)
                {
                    AppendJsonText(languages, language, item);
                }
            }
        }

        private static void AppendText(Dictionary<string, StringBuilder> languages, string language, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!languages.TryGetValue(language, out var stringBuilder))
                {
                    stringBuilder = Pool.Get();

                    languages[language] = stringBuilder;
                }

                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" ");
                }

                stringBuilder.Append(text);
            }
        }
    }
}

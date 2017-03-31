using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmojiSharp;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Text
{
    public class MessageCleanup
    {
        private readonly Dictionary<string, Emoji> complexEmojis;

        private readonly Dictionary<uint, Emoji> emojis;

        private readonly Extractor extractor = new Extractor();

        public MessageCleanup()
        {
            emojis = (from item in Emoji.All.Values
                      where !item.Unified.Contains('-')
                      select item).ToDictionary(item => uint.Parse(item.Unified, NumberStyles.AllowHexSpecifier), emoji => emoji);

            complexEmojis = (from item in Emoji.All.Values
                             select item).ToDictionary(item => item.Unified, emoji => emoji);
        }

        public async Task<string> Cleanup(TweetData message)
        {
            Guard.NotNull(() => message, message);
            return await Task.Run(() => CleanupInternal(message)).ConfigureAwait(false);
        }

        private string CleanupInternal(TweetData message)
        {
            var text = Replace(message.Text.ToLower(), extractor.ExtractUrlsWithIndices(message.Text), string.Empty);
            var names = extractor.ExtractMentionedScreennamesWithIndices(text)
                .Where(item => !item.Value.Contains("trump"))
                .ToArray();
            text = Replace(text, names, "AT_USER");
            StringBuilder builder = new StringBuilder();
            char? previous = null;
            char? previousToPrevious = null;
            for (int i = 0; i < text.Length; i++)
            {
                var letter = text[i];
                bool isTwoStep;
                if (letter == '…' ||
                    letter == '\r')
                {
                    continue;
                }

                if (letter == '\n' ||
                   letter == '\b')
                {
                    letter = ' ';
                }


                var emoji = GetEmoji(text, i, out isTwoStep);
                if (emoji != null)
                {
                    if (builder.Length > 0 &&
                       builder[builder.Length - 1] != ' ')
                    {
                        builder.Append(' ');
                    }

                    builder.AppendFormat($"{emoji} ");
                    if (isTwoStep)
                    {
                        i++;
                    }

                    continue;
                }


                if ((char.IsLetterOrDigit(letter) && letter != previousToPrevious) ||
                   letter != previous)
                {
                    builder.Append(letter);
                }

                previousToPrevious = previous;
                previous = letter;
            }

            text = builder.ToString().Trim();
            return Regex.Replace(text, @"\b(\w+)\s+\1\b", "$1", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private string GetEmoji(string text, int index, out bool isTwoStep)
        {
            Emoji emoji;
            isTwoStep = true;
            if (index < text.Length - 1)
            {
                try
                {
                    int unicodeCodePoint = char.ConvertToUtf32(text, index);
                    if (unicodeCodePoint > 0xffff)
                    {
                        return emojis.TryGetValue((uint)unicodeCodePoint, out emoji) ? emoji.AsShortcode() : null;
                    }
                }
                catch (Exception)
                {
                    isTwoStep = false;
                    return "";
                }
            }

            var letter = text[index];
            var map = $"{Convert.ToUInt16(letter):X4}";
            if (complexEmojis.TryGetValue(map, out emoji))
            {
                isTwoStep = false;
                return emoji.AsShortcode();
            }

            if (index < text.Length - 1)
            {
                map += $"-{Convert.ToUInt16(text[index + 1]):X4}";
                if (complexEmojis.TryGetValue(map, out emoji))
                {
                    return emoji.AsShortcode();
                }
            }

            return null;
        }

        private string Replace(string message, TweetEntity[] entities, string replacement)
        {
            if (entities == null ||
               entities.Length == 0)
            {
                return message;
            }

            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach (var tweetEntity in entities.OrderBy(item => item.Start))
            {
                builder.Append(message.Substring(index, tweetEntity.Start - index));
                builder.Append(replacement);
                index = tweetEntity.End;
            }

            builder.Append(message.Substring(index));
            return builder.ToString();
        }
    }
}

﻿using CommunityToolkit.Mvvm.ComponentModel;
using FluentStore.SDK.Models;
using System.Linq;

namespace FluentStore.SDK.Images
{
    public partial class TextImage : ImageBase
    {
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private string _fontFamily = SharedResources.TextFont;

        public static TextImage CreateFromName(string name, ImageType imageType = ImageType.Unspecified)
        {
            string text = string.Empty;
            if (name.Length <= 4)
            {
                text = name;
                goto done;
            }

            string[] words = name.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                foreach (string word in words)
                {
                    if (word.All(c => !char.IsLetter(c) || char.IsUpper(c)))
                        text += word;
                    else
                        text += word.First(char.IsLetterOrDigit);
                }

                if (words.Length == 3 || words.Length == 4)
                    goto done;
            }
            string noGaps = string.Join(string.Empty, words);

            string noVowels = noGaps.Replace("a", string.Empty)
                                    .Replace("e", string.Empty)
                                    .Replace("i", string.Empty)
                                    .Replace("o", string.Empty)
                                    .Replace("u", string.Empty)
                                    .Replace("y", string.Empty);
            if (noVowels.Length <= 4)
            {
                text = noVowels;
                goto done;
            }

            text = name[..1];
            for (int i = 1; i < noVowels.Length; i++)
            {
                char c = noVowels[i];
                if (char.IsUpper(c))
                    text += c;

            }

        done:
            if (string.IsNullOrWhiteSpace(text))
                text = name.Substring(0, 3);
            return new TextImage
            {
                Text = text,
                ImageType = imageType
            };
        }

        public override string ToString() => Caption ?? Text;
    }
}

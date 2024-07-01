using System;
using System.Text.RegularExpressions;

namespace VZ.Shared
{
    public static class StringExtensions
    {
        public const char ID_DELIMITER = '-';

        public static string Truncate(this string value, int maximumLength, string continuationMarker = "...")
        {
            if (string.IsNullOrEmpty(value) || (value.Length <= maximumLength))
            {
                return value;
            }
            
            var truncatedString = value.Substring(0, maximumLength - continuationMarker.Length);
            return truncatedString + continuationMarker;
        }

        /// <summary>
        /// Extract the ID part of an ID string, which should always be the first part of an ID.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="partIndex"></param>
        /// <returns></returns>
        public static string IdPart(this string value, int partIndex = 1)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            var parts = value.Split(ID_DELIMITER);
            return parts[0];
        }

        /// <summary>
        /// Extract the partitionKey part of an ID string, which should always be the last part of an ID, because sometimes an ID has more than 2 parts (eg., BusinessProducts).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="partIndex"></param>
        /// <returns></returns>
        public static string PartitionKeyPart(this string value, int partIndex = 1)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            var parts = value.Split(ID_DELIMITER);
            return parts[parts.Length - 1];
        }

        public static char IdDelimiter(this string value)
        {
            return ID_DELIMITER;
        }

        public static string Slugify(this string value)
        {
            string result = value.ToLower();
            result = Regex.Replace(result, @"\s+", "-");           // Replace spaces with -
            result = Regex.Replace(result, @"[^\w\-]+", "");       // Remove all non-word chars
            result = Regex.Replace(result, @"\-\-+", "-");         // Replace multiple - with single -
            result = Regex.Replace(result, @"^-+", "");            // Trim - from start of text
            result = Regex.Replace(result, @"-+$", "");            // Trim - from end of text
            return result;
        }
    }
}

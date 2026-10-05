using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TaskControl.Models
{
    // Dates come from <input type="date">, which always posts yyyy-MM-dd.
    // Anything else (a typo, 2026-02-31, text) is rejected here instead of failing in DateTime.Parse.
    public class ValidDate : ValidationAttribute
    {
        public override bool IsValid(object value) =>
            value == null || DateTime.TryParseExact(value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    // Times come from <input type="time">, which posts HH:mm (or HH:mm:ss with a seconds step).
    public class ValidTime : ValidationAttribute
    {
        private static readonly string[] Formats = { @"hh\:mm", @"hh\:mm\:ss" };

        public override bool IsValid(object value) =>
            value == null || TimeSpan.TryParseExact(value.ToString(), Formats, CultureInfo.InvariantCulture, out _);
    }
}

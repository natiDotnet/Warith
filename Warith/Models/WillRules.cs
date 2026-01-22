using System;
using System.Collections.Generic;
using System.Text;

namespace Warith.Models;

public static class WillRules
{
    public static readonly ImmutableArray<string> AllowedFractions =
    [
        "2/3",
        "1/2",
        "1/3",
        "1/4",
        "1/5",
        "1/6",
        "1/7",
        "1/8",
        "1/9",
        "1/10"
    ];

    public static decimal? ParseFraction(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        if (decimal.TryParse(input, out var d))
            return d;

        var parts = input.Split('/');
        if (parts.Length != 2)
            return null;

        if (!decimal.TryParse(parts[0], out var n))
            return null;

        if (!decimal.TryParse(parts[1], out var m) || m == 0)
            return null;

        return n / m;
    }

    public static bool IsValid(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        AllowedFractions.Contains(value);
}


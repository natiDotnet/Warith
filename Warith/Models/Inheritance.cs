using System;
using System.Collections.Generic;
using System.Text;

namespace Warith.Models;

public record Inheritance
{
    // ───────────── Person ─────────────
    public string Gender { get; init; } = "zakar"; // zakar | ontha

    // ───────────── Financials ─────────────
    public decimal? Debt { get; init; }
    public decimal? TotalAmount { get; init; }

    // ───────────── Wills ─────────────
    public string? Will1 { get; init; }
    public string? Will2 { get; init; }
    public string? Will3 { get; init; }

    /// <summary>
    /// Do heirs accept will exceeding 1/3
    /// 0 = No, 1 = Yes (matches backend)
    /// </summary>
    public bool AcceptMoreThanThird { get; init; }

    // ───────────── Spouses ─────────────
    /// <summary>
    /// 0..4
    /// </summary>
    public int SpousesCount { get; init; }

    // ───────────── Parents ─────────────
    public bool MotherAlive { get; init; }
    public bool FatherAlive { get; init; }

    // ───────────── Children ─────────────
    public int Sons { get; init; }
    public int Daughters { get; init; }

    // ───────────── School / Law ─────────────
    /// <summary>
    /// gm | nf | mk | sh | bl
    /// </summary>
    public string School { get; init; } = "gm";

    // ───────────── Derived UI State ─────────────
    public bool CanCalculate =>
        TotalAmount is > 0 &&
        (Debt ?? 0) >= 0 &&
        AreWillsValid();

    private bool AreWillsValid()
    {
        var wills = new[] { Will1, Will2, Will3 }
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(WillRules.ParseFraction)
            .ToList();

        if (wills.Any(w => w is null))
            return false;

        var sum = wills.Sum(w => w!.Value);

        return sum <= (1m / 3m) || AcceptMoreThanThird;
    }
}


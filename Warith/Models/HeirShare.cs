namespace Warith.Models;

public record HeirShare
{
    public string Percent { get; init; }      // %25, %50, etc.
    public string Share { get; init; }        // "1/4", "2/4" etc
    public int Count { get; init; }           // number of heirs
    public string Heir { get; init; }         // "Wife", "Mother", "Father"
}

public record WarethResponse(string Message, IReadOnlyList<HeirShare> Result);

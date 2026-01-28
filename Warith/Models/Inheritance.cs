using System;
using System.Collections.Generic;
using System.Text;

namespace Warith.Models;


public record InheritanceForm
{
    // gender
    public string Gender { get; init; }

    // dyoon
    public decimal? Debt { get; init; }

    // floos
    public decimal? TotalAmount { get; init; }

    // eradyya1
    public string Will1 { get; init; }

    // eradyya2
    public string Will2 { get; init; }

    // eradyya3
    public string Will3 { get; init; }

    // agazo
    public bool? AcceptWillMoreThanOneThird { get; init; }

    // zawgat
    public int? NumberOfWives { get; init; }

    // u41
    public bool IsHusbandAlive { get; init; }

    // m41
    public bool IsWifeAlive { get; init; }

    // om1
    public bool IsMotherAlive { get; init; }

    // ab1
    public bool IsFatherAlive { get; init; }

    // omom1
    public bool IsGrandMotherMaternalAlive { get; init; }

    // abab1
    public bool IsGrandFatherPaternalAlive { get; init; }

    // ababab1
    public bool IsGreatGrandFatherPaternalAlive { get; init; }

    // omomom1
    public bool IsGreatGrandMotherMaternalAlive { get; init; }

    // omab1
    public bool IsGrandMotherPaternalAlive { get; init; }

    // omabab1
    public bool IsMotherOfFatherOfFatherAlive { get; init; }

    // omomab1
    public bool IsMotherOfMotherOfFatherAlive { get; init; }

    // s1
    public int? Sons { get; init; }

    // s2
    public int? Daughters { get; init; }

    // s3
    public int? SonsOfSon { get; init; }

    // s4
    public int? DaughtersOfSon { get; init; }

    // s5
    public int? SonsOfSonOfSon { get; init; }

    // s6
    public int? DaughtersOfSonOfSon { get; init; }

    // s7
    public int? FullBrothers { get; init; }

    // s8
    public int? FullSisters { get; init; }

    // s9
    public int? PaternalBrothers { get; init; }

    // s10
    public int? PaternalSisters { get; init; }

    // s11
    public int? UterineBrothers { get; init; }

    // s12
    public int? UterineSisters { get; init; }

    // s13
    public int? SonsOfFullBrother { get; init; }

    // s14
    public int? SonsOfPaternalBrother { get; init; }

    // s15
    public int? SonsOfSonOfFullBrother { get; init; }

    // s16
    public int? SonsOfSonOfPaternalBrother { get; init; }

    // s17
    public int? FullBrothersOfFather { get; init; }

    // s18
    public int? PaternalBrothersOfFather { get; init; }

    // s19
    public int? SonsOfFullBrotherOfFather { get; init; }

    // s20
    public int? SonsOfPaternalBrotherOfFather { get; init; }

    // s21
    public int? SonsOfSonOfFullBrotherOfFather { get; init; }

    // s22
    public int? SonsOfSonOfPaternalBrotherOfFather { get; init; }

    // s23
    public int? FullBrotherOfGrandFather { get; init; }

    // s24
    public int? PaternalBrotherOfGrandFather { get; init; }

    // s25
    public int? SonsOfFullBrotherOfGrandFather { get; init; }

    // s26
    public int? SonsOfPaternalBrotherOfGrandFather { get; init; }

    // grouphalat[monaskha]
    public bool PredeceasedRelative { get; init; }

    // grouphalat[wasyawagba]
    public bool OffspringOfPredeceased { get; init; }

    // grouphalat (haml)
    public bool Fetus { get; init; }

    // grouphalat (mafkood)
    public bool MissingPerson { get; init; }

    // mazaheb
    public string SchoolOrLaw { get; init; }
}

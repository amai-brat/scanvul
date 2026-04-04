using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Tests.Versions;

public class SoftwareVersionTests
{
    [Theory(DisplayName = "SoftwareVersion correctly compares")]
    [InlineData("1.3.0", "1.3.0 rc3", CompareResult.Greater)] // ошибка у БДУ: https://bdu.fstec.ru/vul/2021-01894
    [InlineData("1:1.8.11", "1.8.3-lp152.1.1", CompareResult.Greater)] // https://bdu.fstec.ru/vul/2023-00353
    [InlineData("0.20.0", "0.20.0 rc2", CompareResult.Greater)] // https://bdu.fstec.ru/vul/2023-01107
    [InlineData("2.1.1767980792+707c12b", "2.1.0 beta3", CompareResult.Greater)] // https://bdu.fstec.ru/vul/2021-01326
    [InlineData("1", "2", CompareResult.Less)] // https://github.com/microsoft/winget-cli/blob/master/doc/specs/%23980%20-%20Apps%20and%20Features%20entries%20version%20mapping.md
    [InlineData("1.0.0", "2.0.0", CompareResult.Less)]
    [InlineData("0.0.1-alpha", "0.0.2-alpha", CompareResult.Less)]
    [InlineData("0.0.1-beta", "0.0.2-alpha", CompareResult.Less)]
    [InlineData("0.0.1-alpha", "0.0.1", CompareResult.Less)]
    [InlineData("13.9.8", "14.0", CompareResult.Less)]
    [InlineData("1.0", "1.0", CompareResult.Equal)]
    public void Compare_InputDataGiven_CorrectlyCompared(
        string first, 
        string second, 
        CompareResult expected)
    {
        // arrange
        if (!SoftwareVersion.TryParse(first, out var firstVersion) || !SoftwareVersion.TryParse(second, out var secondVersion))
            throw new ArgumentException($"Incorrect InlideData: {first}, {second}");
        
        // act
        var actual = firstVersion.CompareTo(secondVersion).ToCompareResult();
        
        // assert
        Assert.Equal(expected, actual);
    }
}
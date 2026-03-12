namespace ScanVul.Server.Tests.Versions;

public enum CompareResult
{
    Equal = 0,
    Less = 1,
    Greater = 2
}

public static class Extensions 
{
    public static CompareResult ToCompareResult(this int number)
    {
        return number switch
        {
              0 => CompareResult.Equal,
            > 0 => CompareResult.Greater,
            < 0 => CompareResult.Less
        };
    }
}
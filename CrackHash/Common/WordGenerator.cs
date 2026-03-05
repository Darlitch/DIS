namespace Common;

public static class WordGenerator
{
    public static IEnumerable<string> GenerateRange(
        string alphabet, int length, long startIndex, long endIndex)
    {
        for (long i = startIndex; i <= endIndex; i++)
        {
            yield return GetWord(i, length, alphabet);
        }
    }
    
    public static string GetWord(long index, int length, string alphabet)
    {
        int baseSize = alphabet.Length;
        var result = new char[length];
        for (var i = length - 1; i >= 0; i--)
        {
            result[i] = alphabet[(int)(index % baseSize)];
            index /= baseSize;
        }
        return new string(result);
    }
    
    public static long CalculateTotalCombinations(int alphabetSize, int length)
    {
        return (long)Math.Pow(alphabetSize, length);
    }
}
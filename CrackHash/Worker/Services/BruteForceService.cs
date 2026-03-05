using Common;

namespace Worker.Services;

public class BruteForceService
{
    public List<String> FindMatches(string targetHash, string alphabet,
        int maxLength, long startIndex, long endIndex)
    {
        var result = new List<string>();
        for (var i = 1; i <= maxLength; i++)
        {
            foreach (var word in WordGenerator.GenerateRange(alphabet, i, startIndex, endIndex))
            {
                var hash = Md5helper.Compute(word);
                if (hash == targetHash)
                {
                    result.Add(word);
                }
            }
        }
        return result;
    }
}
using Common;
using Contract.Xml;

namespace Worker.Services;

public class BruteForceService
{
    public List<String> FindMatches(WorkerTaskRequest request)
    {
        var result = new List<string>();
        var alphabet = string.Concat(request.Alphabet.Symbols);
        for (var i = 1; i <= request.MaxLength; i++)
        {
            long total = WordGenerator.CalculateTotalCombinations(alphabet.Length, i);
            long partSize = total / request.PartCount;
            long startIndex = request.PartNumber * partSize;
            long endIndex = request.PartNumber == request.PartCount-1 ? total - 1 : startIndex + partSize - 1;
            foreach (var word in WordGenerator.GenerateRange(alphabet, i, startIndex, endIndex))
            {
                var hash = Md5helper.Compute(word);
                if (hash == request.Hash)
                {
                    result.Add(word);
                }
            }
        }
        return result;
    }
}
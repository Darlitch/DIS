using Contract.Xml;

namespace Manager.Utilities;

public static class CrackAlphabet
{
    private const string Default = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static Alphabet GetAlphabet()
    {
        return new Alphabet
        {
            Symbols = CrackAlphabet.Default
                .Select(c => c.ToString())
                .ToList()
        };
    }
}
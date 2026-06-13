namespace Timebash.Tests.Unit.Core.TestData;

public class NullOrWhitespaceStringData : TheoryData<string>
{

    public NullOrWhitespaceStringData()
    {
        Add(null!);
        Add(string.Empty);
        Add(" ");
        Add("   ");
        Add("\t");
        Add("\n");
        Add("\r");
        Add("\u00A0");
        Add(" \t\r\n\t");
    }
}

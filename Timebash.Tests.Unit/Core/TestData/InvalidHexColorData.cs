namespace Timebash.Tests.Unit.Core.TestData;

public class InvalidHexColorData : TheoryData<string>
{
    public InvalidHexColorData()
    {
        Add("123456");
        Add("#12345");
        Add("#1234567");
        Add("#12");
        Add("#12345G");
        Add("# 123456");
        Add("#123456 ");
        Add("#123 456");
    }
}
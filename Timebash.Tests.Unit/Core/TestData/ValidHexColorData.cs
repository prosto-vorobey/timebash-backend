namespace Timebash.Tests.Unit.Core.TestData;

public class ValidHexColorData : TheoryData<string>
{
    public ValidHexColorData()
    {
        Add("#000000");
        Add("#FFFFFF");
        Add("#abcdef");
        Add("#ABCDEF");
        Add("#AbCdEf");
        Add("#123456");
        Add("#123");
        Add("#abc");
        Add("#FFF");
    }
}

namespace TuckBar.Tests;

public class TaskbarHelperTests
{
    [Theory]
    [InlineData(0x03, true)]
    [InlineData(0x02, false)]
    [InlineData(0x01, true)]
    [InlineData(0x00, false)]
    public void IsAutoHideEnabled_ReadsCorrectBit(byte flagByte, bool expected)
    {
        var blob = new byte[56];
        blob[8] = flagByte;

        Assert.Equal(expected, TaskbarHelper.IsAutoHideEnabled(blob));
    }

    [Fact]
    public void IsAutoHideEnabled_ReturnsFalse_WhenBlobTooShort()
    {
        var blob = new byte[5];

        Assert.False(TaskbarHelper.IsAutoHideEnabled(blob));
    }

    [Theory]
    [InlineData(0x02, true, 0x03)]
    [InlineData(0x03, false, 0x02)]
    [InlineData(0x02, false, 0x02)]
    [InlineData(0x03, true, 0x03)]
    public void SetAutoHideBit_SetsCorrectBit(byte initialByte, bool autoHide, byte expectedByte)
    {
        var blob = new byte[56];
        blob[8] = initialByte;

        byte[] result = TaskbarHelper.SetAutoHideBit(blob, autoHide);

        Assert.Equal(expectedByte, result[8]);
    }

    [Fact]
    public void SetAutoHideBit_DoesNotMutateOriginal()
    {
        var blob = new byte[56];
        blob[8] = 0x02;

        TaskbarHelper.SetAutoHideBit(blob, true);

        Assert.Equal(0x02, blob[8]);
    }

    [Fact]
    public void SetAutoHideBit_PreservesOtherBytes()
    {
        var blob = new byte[56];
        blob[0] = 0xFF;
        blob[7] = 0xAB;
        blob[8] = 0x02;
        blob[9] = 0xCD;

        byte[] result = TaskbarHelper.SetAutoHideBit(blob, true);

        Assert.Equal(0xFF, result[0]);
        Assert.Equal(0xAB, result[7]);
        Assert.Equal(0xCD, result[9]);
    }
}

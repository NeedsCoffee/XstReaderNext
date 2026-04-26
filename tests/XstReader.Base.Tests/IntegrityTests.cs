using System;
using System.Text;

namespace XstReader.Base.Tests;

public class IntegrityTests
{
    [Fact]
    public void ComputeCrc_ComputesExpectedValue_ForByteRange()
    {
        byte[] buffer = { 1, 2, 3, 4 };

        uint crc = Integrity.ComputeCrc(buffer, 0, 4);

        Assert.Equal(2541233361u, crc);
    }

    [Fact]
    public void ComputeCrc_ComputesExpectedValue_ForAsciiPayload()
    {
        byte[] payload = Encoding.ASCII.GetBytes("123456789");

        uint crc = Integrity.ComputeCrc(payload, 0, payload.Length);

        Assert.Equal(771566984u, crc);
    }

    [Fact]
    public void ComputeCrc_Throws_WhenRangeIsOutsideBuffer()
    {
        byte[] buffer = { 1, 2, 3, 4 };

        Assert.Throws<ArgumentOutOfRangeException>(() => Integrity.ComputeCrc(buffer, 3, 2));
    }

    [Fact]
    public void ComputeSignature_UsesLower32BitsXorFold()
    {
        ushort signature = Integrity.ComputeSignature(0x12345678ul, 0xABCDEF01ul);

        Assert.Equal((ushort)128, signature);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 64)]
    [InlineData(64, 64)]
    [InlineData(65, 128)]
    [InlineData(127, 128)]
    public void AlignTo64_RoundsUpToNearest64ByteBoundary(int value, int expected)
    {
        Assert.Equal(expected, Integrity.AlignTo64(value));
    }
}

namespace XstReader.Base.Tests;

public class CryptoTests
{
    [Fact]
    public void Decrypt_DoesNothing_ForNoneMethod()
    {
        byte[] buffer = { 0x10, 0x20, 0x30 };

        Crypto.Decrypt(ref buffer, EbCryptMethod.NDB_CRYPT_NONE, 0u, 0, buffer.Length);

        Assert.Equal(new byte[] { 0x10, 0x20, 0x30 }, buffer);
    }

    [Fact]
    public void Decrypt_TransformsBuffer_ForPermuteMethod()
    {
        byte[] buffer = { 0x00, 0x01, 0x02 };

        Crypto.Decrypt(ref buffer, EbCryptMethod.NDB_CRYPT_PERMUTE, 0u, 0, buffer.Length);

        Assert.Equal(new byte[] { 0x47, 0xF1, 0xB4 }, buffer);
    }

    [Fact]
    public void Decrypt_ThrowsHelpfulMessage_ForWipProtectedFiles()
    {
        byte[] buffer = { 0x00 };

        var exception = Assert.Throws<XstException>(() =>
            Crypto.Decrypt(ref buffer, EbCryptMethod.NDB_CRYPT_EDPCRYPTED, 0u, 0, buffer.Length));

        Assert.Contains("Windows Information Protection", exception.Message);
    }
}

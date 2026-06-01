using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Core.Tests.Parser;

public sealed class EncodingSpedTests
{
    [Fact]
    public void Latin1_DecodificaCaracteresAcentuados()
    {
        // Bytes Latin1 / Win-1252: 'A' = 0x41, 'ç' = 0xE7, 'ã' = 0xE3, 'o' = 0x6F.
        byte[] bytes = [0x41, 0xE7, 0xE3, 0x6F];

        string texto = EncodingSped.Latin1.GetString(bytes);

        texto.Should().Be("Ação");
    }

    [Fact]
    public void Latin1_RoundTripDeStringPortuguesa()
    {
        const string original = "Inscrição: São Paulo — empresa nº 1";

        var bytes = EncodingSped.Latin1.GetBytes(original);
        var voltou = EncodingSped.Latin1.GetString(bytes);

        voltou.Should().Be(original);
    }

    [Fact]
    public void DelimitadoresAscii_TemValoresEsperados()
    {
        EncodingSped.PipeAscii.Should().Be((byte)'|');
        EncodingSped.LfAscii.Should().Be((byte)'\n');
        EncodingSped.CrAscii.Should().Be((byte)'\r');
    }
}

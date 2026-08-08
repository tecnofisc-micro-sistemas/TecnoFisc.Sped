using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Gerador;

public sealed class EscritorSpedTxtTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static async Task<string> EscreverParaTextoAsync(IEnumerable<RegistroSped> registros)
    {
        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();
        await escritor.WriteAsync(fluxo, registros);
        return EncodingSped.Latin1.GetString(fluxo.ToArray());
    }

    [Fact]
    public async Task EscreverAsync_RegistroSimples_GeraLinhaPipeDelimitadaComCRLF()
    {
        var registro = new Registro0000Sintetico
        {
            CodVer = "006",
            DtIni = new DateOnly(2025, 1, 1),
            DtFin = new DateOnly(2025, 1, 31),
            Nome = "EMPRESA TESTE",
            Cnpj = Cnpj.Create("11222333000181"),
        };

        var saida = await EscreverParaTextoAsync([registro]);

        saida.Should().Be("|0000|006|01012025|31012025|EMPRESA TESTE|11222333000181|\r\n");
    }

    [Fact]
    public async Task EscreverAsync_VariosRegistros_PreservaOrdemDeEntrada()
    {
        var registros = new RegistroSped[]
        {
            new Registro0000Sintetico
            {
                CodVer = "006",
                DtIni = new DateOnly(2025, 1, 1),
                DtFin = new DateOnly(2025, 1, 31),
                Nome = "EMPRESA",
                Cnpj = Cnpj.Create("11222333000181"),
            },
            new RegistroC001Sintetico { IndMov = 0 },
            new RegistroC100Sintetico
            {
                IndOper = "0",
                CodPart = 123,
                VlDoc = 1500.75m,
                Cfop = Cfop.Create("5102"),
            },
            new Registro9999Sintetico { QtdLin = 4 },
        };

        var saida = await EscreverParaTextoAsync(registros);

        saida.Should().Be(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|9999|4|\r\n");
    }

    [Fact]
    public async Task EscreverAsync_StringNula_VirarCampoVazioEntrePipes()
    {
        var registro = new Registro0000Sintetico
        {
            CodVer = "006",
            DtIni = new DateOnly(2025, 1, 1),
            DtFin = new DateOnly(2025, 1, 31),
            Nome = null,
            Cnpj = Cnpj.Create("11222333000181"),
        };

        var saida = await EscreverParaTextoAsync([registro]);

        saida.Should().Be("|0000|006|01012025|31012025||11222333000181|\r\n");
    }

    [Fact]
    public async Task EscreverAsync_DecimalRespeitaCasasDoLayout()
    {
        var registro = new RegistroC100Sintetico
        {
            IndOper = "1",
            CodPart = 1,
            VlDoc = 1234m,
            Cfop = Cfop.Create("5102"),
        };

        var saida = await EscreverParaTextoAsync([registro]);

        saida.Should().Contain("|1234,00|");
    }

    [Fact]
    public async Task EscreverAsync_CaracteresAcentuadosCodificadosEmLatin1()
    {
        var registro = new Registro0000Sintetico
        {
            CodVer = "006",
            DtIni = new DateOnly(2025, 1, 1),
            DtFin = new DateOnly(2025, 1, 31),
            Nome = "AÇÃO LTDA",
            Cnpj = Cnpj.Create("11222333000181"),
        };

        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();
        await escritor.WriteAsync(fluxo, new RegistroSped[] { registro }, TestContext.Current.CancellationToken);

        var bytes = fluxo.ToArray();
        // Latin1 codifica 'Ç' = 0xC7, 'Ã' = 0xC3.
        bytes.Should().Contain((byte)0xC7);
        bytes.Should().Contain((byte)0xC3);
    }

    [Fact]
    public async Task EscreverAsync_QuandoCodigoNaoCatalogado_LancaInvalidOperationException()
    {
        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();
        var registro = new RegistroDesconhecido();

        var act = async () => await escritor.WriteAsync(fluxo, new RegistroSped[] { registro });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ZZZZ*");
    }

    [Fact]
    public async Task EscreverAsync_RegistroSemCamposModelados_LancaInvalidOperationException()
    {
        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();
        var registro = new RegistroZ900SemCamposSintetico();

        var act = async () => await escritor.WriteAsync(fluxo, new RegistroSped[] { registro });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Z900*não tem campos modelados*");
    }

    [Fact]
    public async Task EscreverAsync_QuandoStreamNulo_LancaArgumentNullException()
    {
        var escritor = new EscritorSpedTxt(_catalogo);

        var act = async () => await escritor.WriteAsync(
            saida: null!,
            registros: Array.Empty<RegistroSped>());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EscreverAsync_RegistrosNulos_LancaArgumentNullException()
    {
        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();

        var act = async () => await escritor.WriteAsync(fluxo, registros: (IEnumerable<RegistroSped>)null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EscreverAsync_NaoFechaStreamDoChamador()
    {
        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();

        await escritor.WriteAsync(
            fluxo,
            new RegistroSped[] { new Registro9999Sintetico { QtdLin = 1 } },
            TestContext.Current.CancellationToken);

        // Conseguir continuar gravando após o escritor confirma que o stream segue aberto.
        var act = () => fluxo.WriteByte((byte)'X');
        act.Should().NotThrow();
    }

    private sealed class RegistroDesconhecido : RegistroSped
    {
        public override string Codigo => "ZZZZ";
    }
}

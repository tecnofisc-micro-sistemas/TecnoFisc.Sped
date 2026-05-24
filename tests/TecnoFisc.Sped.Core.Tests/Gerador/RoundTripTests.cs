using System.Text;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Core.Tests._Sintetico;

namespace TecnoFisc.Sped.Core.Tests.Gerador;

/// <summary>
/// Invariante de round-trip do Stage 3: parse → generate → parse precisa preservar o
/// conteúdo (modulo normalização declarada). Testes exercitam textos canônicos para que
/// também valha a igualdade textual após uma única passada de parse + generate.
/// </summary>
public sealed class RoundTripTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static async Task<string> RoundTripTextoAsync(string spedOriginal)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(spedOriginal));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public async Task ArquivoCanonico_ParseGenerate_TextoIdenticoAoOriginal()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA TESTE|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|C170|1|MERCADORIA A|10|750,50|\r\n" +
            "|C170|2|MERCADORIA B|5|750,25|\r\n" +
            "|9999|6|\r\n";

        var resultado = await RoundTripTextoAsync(sped);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task ArquivoCanonico_ParseGenerate_PreservaCaracteresLatin1()
    {
        const string sped =
            "|0000|006|01012025|31012025|AÇÃO COMÉRCIO LTDA|11222333000181|\r\n" +
            "|9999|2|\r\n";

        var resultado = await RoundTripTextoAsync(sped);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task DuasPassadasGenerateParse_ConvergemParaMesmoTexto()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|C100|1|456|2000,00|6101|\r\n" +
            "|9999|4|\r\n";

        var primeiraVolta = await RoundTripTextoAsync(sped);
        var segundaVolta = await RoundTripTextoAsync(primeiraVolta);

        segundaVolta.Should().Be(primeiraVolta);
    }

    [Fact]
    public async Task ArquivoCanonico_ParseGenerate_ProduzMesmoModeloSemantico()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|C170|1|ITEM|10|750,50|\r\n" +
            "|9999|5|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        var ct = TestContext.Current.CancellationToken;

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var primeira = new List<RegistroSped>();
        await foreach (var r in leitor.ReadStreamingAsync(entrada, ct))
            primeira.Add(r);

        using var meio = new MemoryStream();
        await escritor.WriteAsync(meio, primeira, ct);
        meio.Position = 0;

        var segunda = new List<RegistroSped>();
        await foreach (var r in leitor.ReadStreamingAsync(meio, ct))
            segunda.Add(r);

        segunda.Select(r => r.Codigo).Should().Equal(primeira.Select(r => r.Codigo));

        var c100Antes = primeira.OfType<RegistroC100Sintetico>().Single();
        var c100Depois = segunda.OfType<RegistroC100Sintetico>().Single();
        c100Depois.IndOper.Should().Be(c100Antes.IndOper);
        c100Depois.CodPart.Should().Be(c100Antes.CodPart);
        c100Depois.VlDoc.Should().Be(c100Antes.VlDoc);
        c100Depois.Cfop.Should().Be(c100Antes.Cfop);

        var r0000Antes = primeira.OfType<Registro0000Sintetico>().Single();
        var r0000Depois = segunda.OfType<Registro0000Sintetico>().Single();
        r0000Depois.CodVer.Should().Be(r0000Antes.CodVer);
        r0000Depois.DtIni.Should().Be(r0000Antes.DtIni);
        r0000Depois.DtFin.Should().Be(r0000Antes.DtFin);
        r0000Depois.Nome.Should().Be(r0000Antes.Nome);
        r0000Depois.Cnpj.Should().Be(r0000Antes.Cnpj);
    }

    [Fact]
    public async Task ArquivoGrande_RoundTrip_PreservaTodos500RegistrosNaOrdem()
    {
        var construtor = new StringBuilder();
        construtor.Append("|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n");
        construtor.Append("|C001|0|\r\n");
        for (int i = 1; i <= 500; i++)
            construtor.Append($"|C100|0|{i}|1000,00|5102|\r\n");
        construtor.Append("|9999|503|\r\n");

        var sped = construtor.ToString();

        var resultado = await RoundTripTextoAsync(sped);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task ParseGerarComTotalizador_AcrescentaFechadoresEEmiteTextoConsistente()
    {
        // Entrada sem fechadores: o totalizador injeta 0990, C990 e 9999.
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
                CodPart = 1,
                VlDoc = 100m,
                Cfop = Cfop.Create("5102"),
            },
        };

        var comFechadores = TotalizadorBlocos.ComFechadores(
            registros,
            _catalogo,
            (bloco, qtd) => bloco switch
            {
                "0" => new Registro0990Sintetico { QtdLin0 = qtd },
                "C" => new RegistroC990Sintetico { QtdLinC = qtd },
                _ => throw new InvalidOperationException(),
            },
            total => new Registro9999Sintetico { QtdLin = total });

        var escritor = new EscritorSpedTxt(_catalogo);
        using var fluxo = new MemoryStream();
        await escritor.WriteAsync(fluxo, comFechadores, TestContext.Current.CancellationToken);

        var texto = EncodingSped.Latin1.GetString(fluxo.ToArray());

        texto.Should().Be(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|0990|2|\r\n" +
            "|C001|0|\r\n" +
            "|C100|0|1|100,00|5102|\r\n" +
            "|C990|3|\r\n" +
            "|9999|6|\r\n");
    }
}

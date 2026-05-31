using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD200).Assembly);

    [Fact]
    public void Atributo_DeclaraD200_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D200");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD200Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("D200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D200");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "CodSit", "Ser", "Sub", "NumDocIni", "NumDocFin", "Cfop", "DtRef", "VlDoc", "VlDesc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodMod
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // CodSit
        meta.Campos[2].Tamanho.Should().Be(4);
        meta.Campos[2].Obrigatorio.Should().BeFalse();   // Ser
        meta.Campos[3].Tamanho.Should().Be(3);
        meta.Campos[3].Obrigatorio.Should().BeFalse();   // Sub
        meta.Campos[4].Tamanho.Should().Be(9);
        meta.Campos[4].Obrigatorio.Should().BeTrue();    // NumDocIni
        meta.Campos[5].Tamanho.Should().Be(9);
        meta.Campos[5].Obrigatorio.Should().BeTrue();    // NumDocFin
        meta.Campos[6].Tamanho.Should().Be(4);
        meta.Campos[6].Obrigatorio.Should().BeTrue();    // Cfop
        meta.Campos[7].Tamanho.Should().Be(8);
        meta.Campos[7].Obrigatorio.Should().BeTrue();    // DtRef
        meta.Campos[8].Obrigatorio.Should().BeTrue();    // VlDoc
        meta.Campos[9].Obrigatorio.Should().BeFalse();   // VlDesc
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D200".AsSpan(), out var meta);
        var registro = (RegistroD200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "57".AsSpan());         // CodMod
        meta.Campos[1].Definidor(registro, "00".AsSpan());         // CodSit
        meta.Campos[2].Definidor(registro, "001".AsSpan());        // Ser
        meta.Campos[3].Definidor(registro, "A".AsSpan());          // Sub
        meta.Campos[4].Definidor(registro, "1".AsSpan());          // NumDocIni
        meta.Campos[5].Definidor(registro, "100".AsSpan());        // NumDocFin
        meta.Campos[6].Definidor(registro, "3559".AsSpan());       // Cfop
        meta.Campos[7].Definidor(registro, "01012021".AsSpan());   // DtRef
        meta.Campos[8].Definidor(registro, "50000,00".AsSpan());   // VlDoc
        meta.Campos[9].Definidor(registro, "500,00".AsSpan());     // VlDesc

        registro.CodMod.Should().Be("57");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be("A");
        registro.NumDocIni.Should().Be(1L);
        registro.NumDocFin.Should().Be(100L);
        registro.Cfop.Should().Be(Cfop.Create("3559"));
        registro.DtRef.Should().Be(new DateOnly(2021, 1, 1));
        registro.VlDoc.Should().Be(50000m);
        registro.VlDesc.Should().Be(500m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D200".AsSpan(), out var meta);
        var registro = (RegistroD200)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // Ser
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // Sub
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.VlDesc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D200|57|00|001|A|1|100|3559|01012021|50000,00|500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemSerieSemDesconto_PreservaTextoCanonico()
    {
        // CT-e sem série/subsérie definidos e sem desconto.
        const string sped = "|D200|57|00|||1|1|3559|01012021|1000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_BilhetePassagem_PreservaTextoCanonico()
    {
        // Bilhete eletrônico (código 63) com documento regular extemporâneo.
        const string sped = "|D200|63|01|A||500|600|6931|15022021|25000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}

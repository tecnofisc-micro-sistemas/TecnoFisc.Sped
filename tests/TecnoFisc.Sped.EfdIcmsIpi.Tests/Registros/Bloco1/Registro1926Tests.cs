using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.246 - exercita a forma do <see cref="Registro1926"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 295-296): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1926Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1926).Assembly);

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

    [Fact]
    public void Atributo_Declara1926_Nivel5_Bloco1()
    {
        var atributo = typeof(Registro1926).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1926");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1926Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("1926".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1926");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodOr", "VlOr", "DtVcto", "CodRec", "NumProc", "IndProc", "Proc", "TxtCompl", "MesRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
        // Campo 06 (NumProc) Tam 15→60 em V017 (Guia Pratico 3.1.0 itens 8-10).
        meta.Campos.Select(c => c.Tamanho).Should().Equal([3, 0, 8, 0, 60, 1, 0, 0, 6]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 2, 0, 0, 0, 0, 0, 0, 0]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome)
            .Should().Equal(["CodOr", "VlOr", "DtVcto", "CodRec", "MesRef"]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1926".AsSpan(), out var meta);
        var registro = (Registro1926)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "090".AsSpan());
        meta.Campos[1].Definidor(registro, "1250,75".AsSpan());
        meta.Campos[2].Definidor(registro, "10042025".AsSpan());
        meta.Campos[3].Definidor(registro, "1003".AsSpan());
        meta.Campos[4].Definidor(registro, "SUBAP/2025/001".AsSpan());
        meta.Campos[5].Definidor(registro, "2".AsSpan());
        meta.Campos[6].Definidor(registro, "Processo estadual".AsSpan());
        meta.Campos[7].Definidor(registro, "Debito extemporaneo 042025".AsSpan());
        meta.Campos[8].Definidor(registro, "042025".AsSpan());

        registro.CodOr.Should().Be("090");
        registro.VlOr.Should().Be(1250.75m);
        registro.DtVcto.Should().Be(new DateOnly(2025, 4, 10));
        registro.CodRec.Should().Be("1003");
        registro.NumProc.Should().Be("SUBAP/2025/001");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaEstadual);
        registro.Proc.Should().Be("Processo estadual");
        registro.TxtCompl.Should().Be("Debito extemporaneo 042025");
        registro.MesRef.Should().Be("042025");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("1926".AsSpan(), out var meta);
        var registro = (Registro1926)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, Span<char>.Empty);
        meta.Campos[5].Definidor(registro, Span<char>.Empty);
        meta.Campos[6].Definidor(registro, Span<char>.Empty);
        meta.Campos[7].Definidor(registro, Span<char>.Empty);

        registro.NumProc.Should().BeNull();
        registro.IndProc.Should().BeNull();
        registro.Proc.Should().BeNull();
        registro.TxtCompl.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorOrigemProcesso.Sefaz)]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("2", IndicadorOrigemProcesso.JusticaEstadual)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    [InlineData("", null)]
    public void Definidor_IndProc_MapeiaCodigos(string input, IndicadorOrigemProcesso? esperado)
    {
        _catalogo.TentarObter("1926".AsSpan(), out var meta);
        var registro = (Registro1926)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, input.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Theory]
    [InlineData("000")]
    [InlineData("003")]
    [InlineData("004")]
    [InlineData("005")]
    [InlineData("006")]
    [InlineData("090")]
    public async Task RoundTrip_CodOrValido_PreservaTextoCanonico(string codOr)
    {
        var sped = $"|1926|{codOr}|500,00|20032025|1002|||||032025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1926|090|1250,75|10042025|1003|SUBAP/2025/001|2|Processo estadual|Debito extemporaneo 042025|042025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|1926|003|2750,50|10022025|1001|||||022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

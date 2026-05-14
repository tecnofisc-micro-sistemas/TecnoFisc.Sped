using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.218 - exercita a forma do <see cref="Registro1105"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 271): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1105Tests
{
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1105).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_Declara1105_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1105).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1105");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1105ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("1105".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1105");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod",
            "Serie",
            "NumDoc",
            "ChvNfe",
            "DtDoc",
            "CodItem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeFalse();
        meta.Campos[2].Obrigatorio.Should().BeTrue();
        meta.Campos[3].Obrigatorio.Should().BeFalse();
        meta.Campos[4].Obrigatorio.Should().BeTrue();
        meta.Campos[5].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1105".AsSpan(), out var meta);
        var registro = (Registro1105)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "55".AsSpan());
        meta.Campos[1].Definidor(registro, "001".AsSpan());
        meta.Campos[2].Definidor(registro, "000000123".AsSpan());
        meta.Campos[3].Definidor(registro, ChaveNfeValida.AsSpan());
        meta.Campos[4].Definidor(registro, "15032024".AsSpan());
        meta.Campos[5].Definidor(registro, "ITEM001".AsSpan());

        registro.CodMod.Should().Be(ModeloDocumento.Criar("55"));
        registro.Serie.Should().Be("001");
        registro.NumDoc.Should().Be(123);
        registro.ChvNfe.Should().Be(ChaveAcesso.Criar(ChaveNfeValida));
        registro.DtDoc.Should().Be(new DateOnly(2024, 3, 15));
        registro.CodItem.Should().Be("ITEM001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1105".AsSpan(), out var meta);
        var registro = (Registro1105)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty);
        meta.Campos[3].Definidor(registro, Span<char>.Empty);

        registro.Serie.Should().BeNull();
        registro.ChvNfe.Should().BeNull();
    }

    [Theory]
    [InlineData("01")]
    [InlineData("55")]
    public void Definidor_CodMod_MapeiaModelosValidosDoRegistro(string valor)
    {
        _catalogo.TentarObter("1105".AsSpan(), out var meta);
        var registro = (Registro1105)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.CodMod.Should().Be(ModeloDocumento.Criar(valor));
    }

    [Fact]
    public void Serializar_CodMod_RetornaCodigo()
    {
        _catalogo.TentarObter("1105".AsSpan(), out var meta);
        var registro = (Registro1105)meta!.Fabrica();
        registro.CodMod = ModeloDocumento.Criar("55");

        meta.Campos[0].Serializar(registro).Should().Be("55");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|1105|55|001|123|{ChaveNfeValida}|15032024|ITEM001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_NotaFiscalPapelSemChave_PreservaTextoCanonico()
    {
        const string sped = "|1105|01||456||20042024|ITEMEXP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

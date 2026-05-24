using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.221 - exercita a forma do <see cref="Registro1210"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 273): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1210Tests
{
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1210).Assembly);

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
    public void Atributo_Declara1210_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1210ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("1210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1210");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "TipoUtil",
            "NrDoc",
            "VlCredUtil",
            "ChvDoce",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 4));
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeFalse();
        meta.Campos[2].Obrigatorio.Should().BeTrue();
        meta.Campos[3].Obrigatorio.Should().BeFalse();
        meta.Campos[0].Tamanho.Should().Be(4);
        meta.Campos[2].Decimais.Should().Be(2);
        meta.Campos[3].Tamanho.Should().Be(44);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1210".AsSpan(), out var meta);
        var registro = (Registro1210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP01".AsSpan());
        meta.Campos[1].Definidor(registro, "DOC123".AsSpan());
        meta.Campos[2].Definidor(registro, "125,50".AsSpan());
        meta.Campos[3].Definidor(registro, ChaveNfeValida.AsSpan());

        registro.TipoUtil.Should().Be("SP01");
        registro.NrDoc.Should().Be("DOC123");
        registro.VlCredUtil.Should().Be(125.50m);
        registro.ChvDoce.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1210".AsSpan(), out var meta);
        var registro = (Registro1210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);
        meta.Campos[1].Definidor(registro, Span<char>.Empty);
        meta.Campos[3].Definidor(registro, Span<char>.Empty);

        registro.TipoUtil.Should().BeNull();
        registro.NrDoc.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|1210|SP01|DOC123|125,50|{ChaveNfeValida}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDocumentoEletronico_PreservaTextoCanonico()
    {
        const string sped = "|1210|MG99||10,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.219 - exercita a forma do <see cref="Registro1110"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 271-272): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1110Tests
{
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1110).Assembly);

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
    public void Atributo_Declara1110_Nivel4_Bloco1()
    {
        var atributo = typeof(Registro1110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1110");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1110ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("1110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1110");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart",
            "CodMod",
            "Ser",
            "NumDoc",
            "DtDoc",
            "ChvNfe",
            "NrMemo",
            "Qtd",
            "Unid",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeTrue();
        meta.Campos[2].Obrigatorio.Should().BeFalse();
        meta.Campos[3].Obrigatorio.Should().BeTrue();
        meta.Campos[4].Obrigatorio.Should().BeTrue();
        meta.Campos[5].Obrigatorio.Should().BeFalse();
        meta.Campos[6].Obrigatorio.Should().BeFalse();
        meta.Campos[7].Obrigatorio.Should().BeTrue();
        meta.Campos[8].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1110".AsSpan(), out var meta);
        var registro = (Registro1110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FORN001".AsSpan());
        meta.Campos[1].Definidor(registro, "55".AsSpan());
        meta.Campos[2].Definidor(registro, "001".AsSpan());
        meta.Campos[3].Definidor(registro, "000000123".AsSpan());
        meta.Campos[4].Definidor(registro, "15032024".AsSpan());
        meta.Campos[5].Definidor(registro, ChaveNfeValida.AsSpan());
        meta.Campos[6].Definidor(registro, "987654321".AsSpan());
        meta.Campos[7].Definidor(registro, "10,500".AsSpan());
        meta.Campos[8].Definidor(registro, "UN".AsSpan());

        registro.CodPart.Should().Be("FORN001");
        registro.CodMod.Should().Be(ModeloDocumento.Criar("55"));
        registro.Ser.Should().Be("001");
        registro.NumDoc.Should().Be(123);
        registro.DtDoc.Should().Be(new DateOnly(2024, 3, 15));
        registro.ChvNfe.Should().Be(ChaveAcesso.Criar(ChaveNfeValida));
        registro.NrMemo.Should().Be(987654321);
        registro.Qtd.Should().Be(10.500m);
        registro.Unid.Should().Be("UN");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1110".AsSpan(), out var meta);
        var registro = (Registro1110)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);
        meta.Campos[5].Definidor(registro, Span<char>.Empty);
        meta.Campos[6].Definidor(registro, Span<char>.Empty);

        registro.Ser.Should().BeNull();
        registro.ChvNfe.Should().BeNull();
        registro.NrMemo.Should().BeNull();
    }

    [Theory]
    [InlineData("01")]
    [InlineData("1B")]
    [InlineData("04")]
    [InlineData("55")]
    public void Definidor_CodMod_MapeiaModelosValidosDoRegistro(string valor)
    {
        _catalogo.TentarObter("1110".AsSpan(), out var meta);
        var registro = (Registro1110)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, valor.AsSpan());

        registro.CodMod.Should().Be(ModeloDocumento.Criar(valor));
    }

    [Fact]
    public void Serializar_CodMod_RetornaCodigo()
    {
        _catalogo.TentarObter("1110".AsSpan(), out var meta);
        var registro = (Registro1110)meta!.Fabrica();
        registro.CodMod = ModeloDocumento.Criar("1B");

        meta.Campos[1].Serializar(registro).Should().Be("1B");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|1110|FORN001|55|001|123|15032024|{ChaveNfeValida}|987654321|10,500|UN|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_NotaFiscalPapelSemChaveNemMemorando_PreservaTextoCanonico()
    {
        const string sped = "|1110|FORN002|01||456|20042024|||2,000|KG|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

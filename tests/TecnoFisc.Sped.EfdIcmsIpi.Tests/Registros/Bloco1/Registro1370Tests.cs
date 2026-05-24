using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.229 - exercita a forma do <see cref="Registro1370"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 279): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1370Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1370).Assembly);

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
    public void Atributo_Declara1370_Nivel3_Bloco1()
    {
        var atributo = (RegistroSpedAttribute?)Attribute.GetCustomAttribute(
            typeof(Registro1370),
            typeof(RegistroSpedAttribute));

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1370");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1370ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("1370".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1370");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumBico",
            "CodItem",
            "NumTanque",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([3, 60, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1370".AsSpan(), out var meta);
        var registro = (Registro1370)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12".AsSpan());
        meta.Campos[1].Definidor(registro, "COMB-DIESEL-S10".AsSpan());
        meta.Campos[2].Definidor(registro, "003".AsSpan());

        registro.NumBico.Should().Be(12);
        registro.CodItem.Should().Be("COMB-DIESEL-S10");
        registro.NumTanque.Should().Be("003");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuZero()
    {
        _catalogo.TentarObter("1370".AsSpan(), out var meta);
        var registro = (Registro1370)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumBico.Should().Be(0);
        registro.CodItem.Should().BeNull();
        registro.NumTanque.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1370|12|COMB-DIESEL-S10|003|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTanqueReservatorio_PreservaTextoCanonico()
    {
        const string sped = "|1370|990|GASOLINA-COMUM|101|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

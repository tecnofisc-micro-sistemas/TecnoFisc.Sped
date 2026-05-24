using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.015 — exercita a forma do <see cref="Registro0300"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 39): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0300).Assembly);

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
    public void Atributo_Declara0300_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0300");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0300Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("0300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0300");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodIndBem",
            "IdentMerc",
            "DescrItem",
            "CodPrnc",
            "CodCta",
            "NrParc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0300".AsSpan(), out var meta);
        var registro = (Registro0300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "MAQUINA-001".AsSpan());
        meta.Campos[1].Definidor(registro, "1".AsSpan());
        meta.Campos[2].Definidor(registro, "Maquina de embalagem modelo XYZ".AsSpan());
        meta.Campos[3].Definidor(registro, "MAQUINA-000".AsSpan());
        meta.Campos[4].Definidor(registro, "1.2.01.001".AsSpan());
        meta.Campos[5].Definidor(registro, "48".AsSpan());

        registro.CodIndBem.Should().Be("MAQUINA-001");
        registro.IdentMerc.Should().Be(IdentificadorMercadoriaAtivo.Bem);
        registro.DescrItem.Should().Be("Maquina de embalagem modelo XYZ");
        registro.CodPrnc.Should().Be("MAQUINA-000");
        registro.CodCta.Should().Be("1.2.01.001");
        registro.NrParc.Should().Be(48);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0300".AsSpan(), out var meta);
        var registro = (Registro0300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodIndBem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0300|MAQUINA-001|1|Maquina de embalagem modelo XYZ|MAQUINA-000|1.2.01.001|48|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|0300|MAQUINA-002|1|Prensa hidraulica||1.2.02.001||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("1", IdentificadorMercadoriaAtivo.Bem)]
    [InlineData("2", IdentificadorMercadoriaAtivo.Componente)]
    public void Definidor_IdentMerc_MapeiaCodigo(string codigo, IdentificadorMercadoriaAtivo esperado)
    {
        _catalogo.TentarObter("0300".AsSpan(), out var meta);
        var registro = (Registro0300)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IdentMerc.Should().Be(esperado);
    }
}

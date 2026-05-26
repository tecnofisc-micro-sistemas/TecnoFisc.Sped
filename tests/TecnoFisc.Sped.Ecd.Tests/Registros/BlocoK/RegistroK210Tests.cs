using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.064 — exercita a forma do <see cref="RegistroK210"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 222–223): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraK210_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK210Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("K210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K210");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodEmp", "CodCtaEmp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K210".AsSpan(), out var meta);
        var registro = (RegistroK210)meta!.Fabrica();

        // Exemplo do manual (p. 223): |K210|1234|1.01.01.01|
        meta.Campos[0].Definidor(registro, "1234".AsSpan());
        meta.Campos[1].Definidor(registro, "1.01.01.01".AsSpan());

        registro.CodEmp.Should().Be(1234);
        registro.CodCtaEmp.Should().Be("1.01.01.01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K210".AsSpan(), out var meta);
        var registro = (RegistroK210)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, "".AsSpan());

        registro.CodCtaEmp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 223): empresa 1234 mapeando conta 1.01.01.01
        const string sped = "|K210|1234|1.01.01.01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CodigoContaSimples_PreservaTextoCanonico()
    {
        // Conta com código numérico simples sem hierarquia de pontos
        const string sped = "|K210|5678|101|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

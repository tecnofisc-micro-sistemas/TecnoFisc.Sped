using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.067 — exercita a forma do <see cref="RegistroK315"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 226–228): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK315Tests
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
    public void Atributo_DeclaraK315_Nivel5_BlocoK()
    {
        var atributo = typeof(RegistroK315).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K315");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK315Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("K315".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K315");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["EmpCodContra", "CodContra", "Valor", "IndValor"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K315".AsSpan(), out var meta);
        var registro = (RegistroK315)meta!.Fabrica();

        // Exemplo do manual (p. 228): |K315|5678|2.01.02.01.02|100,00|D|
        meta.Campos[0].Definidor(registro, "5678".AsSpan());
        meta.Campos[1].Definidor(registro, "2.01.02.01.02".AsSpan());
        meta.Campos[2].Definidor(registro, "100,00".AsSpan());
        meta.Campos[3].Definidor(registro, "D".AsSpan());

        registro.EmpCodContra.Should().Be(5678);
        registro.CodContra.Should().Be("2.01.02.01.02");
        registro.Valor.Should().Be(100.00m);
        registro.IndValor.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 228): empresa 5678, conta 2.01.02.01.02, R$ 100,00 Devedor
        const string sped = "|K315|5678|2.01.02.01.02|100,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComIndicadorCredor_PreservaTextoCanonico()
    {
        // Empresa contraparte com parcela credora e conta diferente
        const string sped = "|K315|1234|1.01.01.02|250,50|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

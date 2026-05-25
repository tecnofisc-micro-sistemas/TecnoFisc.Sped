using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.029 — exercita a forma do <see cref="RegistroI075"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 129–130): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI075Tests
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
    public void Atributo_DeclaraI075_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI075).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I075");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI075Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("I075".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I075");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodHist", "DescrHist",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I075".AsSpan(), out var meta);
        var registro = (RegistroI075)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345".AsSpan());
        meta.Campos[1].Definidor(registro, "PAGAMENTO A FORNECEDORES".AsSpan());

        registro.CodHist.Should().Be("12345");
        registro.DescrHist.Should().Be("PAGAMENTO A FORNECEDORES");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 130): COD_HIST=12345, DESCR_HIST=PAGAMENTO A FORNECEDORES
        const string sped = "|I075|12345|PAGAMENTO A FORNECEDORES|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComOutroHistorico_PreservaTextoCanonico()
    {
        const string sped = "|I075|REC001|RECEBIMENTO DE CLIENTES|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

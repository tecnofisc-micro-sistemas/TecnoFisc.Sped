using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.014 — exercita a forma do <see cref="RegistroD761"/> (FCP da escrituração
/// consolidada NFCom, código 62) contra o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 221, Subseção 12):
/// metadados de catálogo, mapeamento de campos e invariante de round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD761Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD761).Assembly);

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
    public void Atributo_DeclaraD761_Nivel4_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD761).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D761");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD761_ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("D761".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D761");
        meta.Campos.Select(c => c.Nome).Should().Equal(["VlFcpOp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D761".AsSpan(), out var meta);
        var registro = (RegistroD761)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "125,50".AsSpan());

        registro.VlFcpOp.Should().Be(125.50m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // FCP único campo obrigatório, valor positivo informativo.
        const string sped = "|D761|125,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

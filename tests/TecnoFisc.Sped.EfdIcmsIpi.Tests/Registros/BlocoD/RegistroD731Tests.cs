using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.009 — exercita a forma do <see cref="RegistroD731"/> (FCP NFCom, código 62)
/// contra o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 217, Subseção 12): metadados de catálogo,
/// mapeamento de campos e invariante de round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD731Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD731).Assembly);

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
    public void Atributo_DeclaraD731_Nivel4_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD731).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D731");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD731Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("D731".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D731");
        meta.Campos.Select(c => c.Nome).Should().Equal(["VlFcpOp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D731".AsSpan(), out var meta);
        var registro = (RegistroD731)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "125,50".AsSpan());

        registro.VlFcpOp.Should().Be(125.50m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // FCP único campo obrigatório, valor positivo informativo.
        const string sped = "|D731|125,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

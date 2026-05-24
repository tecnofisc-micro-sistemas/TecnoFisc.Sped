using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.010 — exercita a forma do <see cref="RegistroD735"/> (observações do lançamento
/// fiscal NFCom, código 62) contra o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 217, Subseção 12):
/// metadados de catálogo, mapeamento de campos e invariante de round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD735Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD735).Assembly);

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
    public void Atributo_DeclaraD735_Nivel3_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD735).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D735");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD735Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("D735".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D735");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodObs", "TxtCompl"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D735".AsSpan(), out var meta);
        var registro = (RegistroD735)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "OBS001".AsSpan());

        registro.CodObs.Should().Be("OBS001");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|D735|OBS001|Ajuste decorrente de legislacao estadual.|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // TXT_COMPL vazio (OC).
        const string sped = "|D735|OBS001||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.017.005 — exercita a forma do <see cref="RegistroC895"/> (observações do
/// lançamento fiscal — Resumo Diário do CF-e-SAT, código 59) contra o Guia Prático EFD-ICMS/IPI
/// V3.2.2 (p. 169, Subseção 12): metadados de catálogo, mapeamento de campos e invariante de
/// round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroC895Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC895).Assembly);

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
    public void Atributo_DeclaraC895_Nivel3_BlocoC_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroC895).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C895");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC895Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C895".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C895");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodObs", "TxtCompl"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("C895".AsSpan(), out var meta);
        var registro = (RegistroC895)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "OBS059".AsSpan());

        registro.CodObs.Should().Be("OBS059");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C895|OBS059|Observacao do resumo diario decorrente de legislacao estadual.|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // TXT_COMPL vazio (OC).
        const string sped = "|C895|OBS059||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

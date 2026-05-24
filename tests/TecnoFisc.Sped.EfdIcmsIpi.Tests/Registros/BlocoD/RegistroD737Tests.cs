using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.011 — exercita a forma do <see cref="RegistroD737"/> (outras obrigações
/// tributárias / ajustes NFCom, código 62) contra o Guia Prático EFD-ICMS/IPI V3.2.2
/// (p. 218, Subseção 12): metadados de catálogo, mapeamento de campos e invariante de
/// round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD737Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD737).Assembly);

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
    public void Atributo_DeclaraD737_Nivel4_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD737).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D737");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD737Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("D737".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D737");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodAj", "DescrComplAj", "CodItem",
            "VlBcIcms", "AliqIcms", "VlIcms", "VlOutros",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D737".AsSpan(), out var meta);
        var registro = (RegistroD737)meta!.Fabrica();

        // Único campo obrigatório (O/O): COD_AJ. Demais são OC/OC.
        meta.Campos[0].Definidor(registro, "SP00000001".AsSpan());

        registro.CodAj.Should().Be("SP00000001");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Ajuste com todos os campos opcionais preenchidos: descrição, item, BC, alíquota,
        // ICMS e outros valores.
        const string sped =
            "|D737|SP00000001|Ajuste descricao completa|ITEM01|1000,00|12,00|120,00|50,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas COD_AJ obrigatório; demais campos vazios (OC).
        const string sped = "|D737|SP00000001|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

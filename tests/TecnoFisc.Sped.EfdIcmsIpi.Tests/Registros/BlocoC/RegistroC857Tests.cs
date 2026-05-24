using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.017.004 — exercita a forma do <see cref="RegistroC857"/> (outras obrigações
/// tributárias / ajustes CF-e-SAT, código 59) contra o Guia Prático EFD-ICMS/IPI V3.2.2
/// (p. 162-163, Subseção 12): metadados de catálogo, mapeamento de campos e invariante de
/// round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroC857Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC857).Assembly);

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
    public void Atributo_DeclaraC857_Nivel4_BlocoC_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroC857).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C857");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC857Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C857".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C857");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodAj", "DescrComplAj", "CodItem",
            "VlBcIcms", "AliqIcms", "VlIcms", "VlOutros",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("C857".AsSpan(), out var meta);
        var registro = (RegistroC857)meta!.Fabrica();

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
            "|C857|SP00000001|Ajuste descricao completa|ITEM01|1000,00|12,00|120,00|50,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas COD_AJ obrigatório; demais campos vazios (OC).
        const string sped = "|C857|SP00000001|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

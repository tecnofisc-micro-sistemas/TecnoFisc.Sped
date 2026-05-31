using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.017.002 — exercita a forma do <see cref="Registro0221"/> (correlação entre códigos
/// de itens comercializados) contra o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 42, Subseção 12):
/// metadados de catálogo, mapeamento de campos e invariante de round-trip parse → texto idêntico.
/// </summary>
public sealed class Registro0221Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0221).Assembly);

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
    public void Atributo_Declara0221_Nivel3_Bloco0_IntroduzidoEmV017()
    {
        var atributo = typeof(Registro0221).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0221");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0221Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("0221".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0221");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodItemAtomico", "QtdContida"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("0221".AsSpan(), out var meta);
        var registro = (Registro0221)meta!.Fabrica();

        // Ambos os campos são O/O.
        meta.Campos[0].Definidor(registro, "ITEM_ATOMICO_001".AsSpan());
        meta.Campos[1].Definidor(registro, "12,000000".AsSpan());

        registro.CodItemAtomico.Should().Be("ITEM_ATOMICO_001");
        registro.QtdContida.Should().Be(12m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do guia: caixa com 12 latas de refrigerante; QTD_CONTIDA = 12.
        const string sped =
            "|0221|LATA_350ML|12,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ItemAtomicoIgualAoPai_QtdContidaUm_PreservaTextoCanonico()
    {
        // Exemplo do guia: lata de refrigerante (item atômico igual ao pai); QTD_CONTIDA = 1.
        const string sped =
            "|0221|LATA_350ML|1,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

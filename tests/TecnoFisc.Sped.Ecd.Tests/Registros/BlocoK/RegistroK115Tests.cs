using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.062 — exercita a forma do <see cref="RegistroK115"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 218–219): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK115Tests
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
    public void Atributo_DeclaraK115_Nivel5_BlocoK()
    {
        var atributo = typeof(RegistroK115).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K115");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK115Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("K115".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K115");
        meta.Campos.Select(c => c.Nome).Should().Equal(["EmpCodPart", "CondPart", "PerEvt"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K115".AsSpan(), out var meta);
        var registro = (RegistroK115)meta!.Fabrica();

        // Exemplo do manual (p. 219): |K115|1234|1|50,0000|
        meta.Campos[0].Definidor(registro, "1234".AsSpan());
        meta.Campos[1].Definidor(registro, "1".AsSpan());
        meta.Campos[2].Definidor(registro, "50,0000".AsSpan());

        registro.EmpCodPart.Should().Be(1234);
        registro.CondPart.Should().Be(CondicaoParticipanteEvento.Sucessora);
        registro.PerEvt.Should().Be(50.0000m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 219) com 4 casas decimais conforme Decimais=4
        const string sped = "|K115|1234|1|50,0000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CondPartAlienante_PreservaTextoCanonico()
    {
        // Alienante (3) — compatível com K110.EVENTO = 2 (Alienação)
        const string sped = "|K115|5678|3|100,0000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

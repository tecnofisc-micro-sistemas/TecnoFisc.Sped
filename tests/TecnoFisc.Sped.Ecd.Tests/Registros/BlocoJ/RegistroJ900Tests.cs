using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.053 — exercita a forma do <see cref="RegistroJ900"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 195–197): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ900Tests
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
    public void Atributo_DeclaraJ900_Nivel2_BlocoJ()
    {
        var atributo = typeof(RegistroJ900).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J900");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ900Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("J900".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J900");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DnrcEncer", "NumOrd", "NatLivro", "Nome", "QtdLin", "DtIniEscr", "DtFinEscr",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J900".AsSpan(), out var meta);
        var registro = (RegistroJ900)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "TERMO DE ENCERRAMENTO".AsSpan()); // DnrcEncer
        meta.Campos[1].Definidor(registro, "100".AsSpan());                   // NumOrd
        meta.Campos[2].Definidor(registro, "DIÁRIO GERAL".AsSpan());          // NatLivro
        meta.Campos[3].Definidor(registro, "EMPRESA TESTE".AsSpan());         // Nome
        meta.Campos[4].Definidor(registro, "500".AsSpan());                   // QtdLin
        meta.Campos[5].Definidor(registro, "01012023".AsSpan());              // DtIniEscr
        meta.Campos[6].Definidor(registro, "31012023".AsSpan());              // DtFinEscr

        registro.DnrcEncer.Should().Be("TERMO DE ENCERRAMENTO");
        registro.NumOrd.Should().Be(100);
        registro.NatLivro.Should().Be("DIÁRIO GERAL");
        registro.Nome.Should().Be("EMPRESA TESTE");
        registro.QtdLin.Should().Be(500);
        registro.DtIniEscr.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFinEscr.Should().Be(new DateOnly(2023, 1, 31));
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 197
        const string sped = "|J900|TERMO DE ENCERRAMENTO|100|DIÁRIO GERAL|EMPRESA TESTE|500|01012023|31012023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DiarioComNomeCompleto_PreservaTextoCanonico()
    {
        const string sped = "|J900|TERMO DE ENCERRAMENTO|1|LIVRO DIÁRIO - ESCRITURAÇÃO CONTÁBIL DIGITAL|COMPANHIA BRASILEIRA DE TECNOLOGIA S.A.|2500|01012022|31122022|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

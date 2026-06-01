using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.Core.Tests.Catalogo;

/// <summary>
/// Valida que o <see cref="CatalogoBuilder"/> rejeita, na construção do catálogo, registros com
/// campo-arquivo (<see cref="CampoSpedAttribute.CampoArquivo"/>) mal-configurados — fail-fast em vez
/// de produzir parse silenciosamente errado em runtime. As regras refletem o que o reassembly do
/// <c>LeitorSpedTxt</c> consegue interpretar: um único campo-arquivo, do tipo string, penúltimo
/// (seguido só pelo terminador), e em par com <see cref="RegistroSpedAttribute.TokenFimArquivo"/>.
///
/// Os tipos sintéticos abaixo não levam <c>[RegistroSped]</c> (não entram no scan de assembly) e são
/// validados via <see cref="CatalogoBuilder.BuildMetadataForType"/>, que recebe o token explicitamente.
/// </summary>
public sealed class CatalogoBuilderCampoArquivoTests
{
    private static MetadadosRegistro Construir<T>(string? tokenFimArquivo) where T : RegistroSped, new()
        => CatalogoBuilder.BuildMetadataForType(typeof(T), "X999", 3, "X", static () => new T(), tokenFimArquivo: tokenFimArquivo);

    [Fact]
    public void CampoArquivoNaoString_Lanca()
    {
        var act = () => Construir<RegArquivoNaoString>("X999FIM");

        act.Should().Throw<InvalidOperationException>().WithMessage("*string*");
    }

    [Fact]
    public void CampoArquivoNaoPenultimo_Lanca()
    {
        var act = () => Construir<RegArquivoNaoPenultimo>("X999FIM");

        act.Should().Throw<InvalidOperationException>().WithMessage("*penúltimo*");
    }

    [Fact]
    public void CampoArquivoSemToken_Lanca()
    {
        var act = () => Construir<RegArquivoSemToken>(tokenFimArquivo: null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*TokenFimArquivo*");
    }

    [Fact]
    public void TokenSemCampoArquivo_Lanca()
    {
        var act = () => Construir<RegSemCampoArquivo>("X999FIM");

        act.Should().Throw<InvalidOperationException>().WithMessage("*TokenFimArquivo*");
    }

    [Fact]
    public void DoisCamposArquivo_Lanca()
    {
        var act = () => Construir<RegDoisCamposArquivo>("X999FIM");

        act.Should().Throw<InvalidOperationException>().WithMessage("*mais de um*");
    }

    [Fact]
    public void ShapeValido_NaoLanca()
    {
        var act = () => Construir<RegArquivoValido>("X999FIM");

        act.Should().NotThrow();
    }

    // ---- tipos sintéticos (sem [RegistroSped]: ignorados pelo scan de assembly) ----

    private sealed class RegArquivoNaoString : RegistroSped
    {
        public override string Codigo => "X999";
        [CampoSped(Ordem = 2)] public string? A { get; set; }
        [CampoSped(Ordem = 3, CampoArquivo = true)] public int Arq { get; set; } // não-string
        [CampoSped(Ordem = 4)] public string? Fim { get; set; }
    }

    private sealed class RegArquivoNaoPenultimo : RegistroSped
    {
        public override string Codigo => "X999";
        [CampoSped(Ordem = 2, CampoArquivo = true)] public string? Arq { get; set; } // 2 campos depois
        [CampoSped(Ordem = 3)] public string? B { get; set; }
        [CampoSped(Ordem = 4)] public string? Fim { get; set; }
    }

    private sealed class RegArquivoSemToken : RegistroSped
    {
        public override string Codigo => "X999";
        [CampoSped(Ordem = 2)] public string? A { get; set; }
        [CampoSped(Ordem = 3, CampoArquivo = true)] public string? Arq { get; set; }
        [CampoSped(Ordem = 4)] public string? Fim { get; set; }
    }

    private sealed class RegSemCampoArquivo : RegistroSped
    {
        public override string Codigo => "X999";
        [CampoSped(Ordem = 2)] public string? A { get; set; }
        [CampoSped(Ordem = 3)] public string? B { get; set; }
    }

    private sealed class RegDoisCamposArquivo : RegistroSped
    {
        public override string Codigo => "X999";
        [CampoSped(Ordem = 2, CampoArquivo = true)] public string? Arq1 { get; set; }
        [CampoSped(Ordem = 3, CampoArquivo = true)] public string? Arq2 { get; set; }
        [CampoSped(Ordem = 4)] public string? Fim { get; set; }
    }

    private sealed class RegArquivoValido : RegistroSped
    {
        public override string Codigo => "X999";
        [CampoSped(Ordem = 2)] public string? A { get; set; }
        [CampoSped(Ordem = 3, CampoArquivo = true)] public string? Arq { get; set; }
        [CampoSped(Ordem = 4)] public string? Fim { get; set; }
    }
}

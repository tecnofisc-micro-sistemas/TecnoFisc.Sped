using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

/// <summary>
/// Prova que <see cref="LeitorSpedTxt"/> aplica a política de domínio de enum (Tasks 1/2) ao
/// interpretar uma linha real — não só que <see cref="ReadingOptions"/> resolve a flag.
/// </summary>
/// <remarks>
/// O caso "aborta a leitura" usa <see cref="LeitorSpedTxt.ReadStreamingAsync"/>, não
/// <see cref="LeitorSpedTxt.ParseLinha"/>: <c>ParseLinha</c> é leniente por contrato — força
/// <c>LenientFieldParsing</c> internamente e nunca lança para erro de campo, mesmo com
/// <see cref="ReadingOptions.LenientFieldParsing"/> desligado (ver
/// <c>LeitorSpedTxtParseLinhaTests.ParseLinha_CampoRuim_RetornaSucessoComErroNoRegistro</c>) —
/// então não serve para provar aborto.
/// </remarks>
public sealed class ValidacaoDominioEnumTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroEnumDominioSintetico).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    [Fact]
    public void OpcoesPadrao_LeemCodigoForaDoDominioSemErro()
    {
        var resultado = new LeitorSpedTxt(_catalogo).ParseLinha("|A200|12|S|0|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroEnumDominioSintetico>().Which;
        ((int)registro.TipoItem).Should().Be(12);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public async Task ComValidacaoLigada_CodigoForaDoDominioAbortaALeitura()
    {
        var opcoes = new ReadingOptions { ValidarDominioDeEnum = true };
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);

        var acao = async () =>
        {
            await foreach (var _ in leitor.ReadStreamingAsync(FluxoSped("|A200|12|S|0|\r\n")))
            {
            }
        };

        await acao.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public void ComValidacaoLigadaELeniente_AcumulaErroEContinua()
    {
        var opcoes = new ReadingOptions { ValidarDominioDeEnum = true, LenientFieldParsing = true };

        // SITUACAO usa "0", não "S": SituacaoItemSintetica também é um enum numérico fechado sem
        // [SpedValor] (existe para provar, em outro teste, o parsing por nome do caminho
        // permissivo) — sob validação estrita ela também ganha definidor estrito, e "S" não é
        // numérico, o que falharia por um motivo alheio ao que este teste prova (TIPO_ITEM fora
        // do domínio). "0" é válido nos dois caminhos (permissivo e estrito) e mantém o teste
        // focado num único campo malformado.
        var resultado = new LeitorSpedTxt(_catalogo, opcoes).ParseLinha("|A200|12|0|0|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.ErrosDeFormato.Should().ContainSingle()
            .Which.Campo.Should().Be("TIPO_ITEM");
    }
}

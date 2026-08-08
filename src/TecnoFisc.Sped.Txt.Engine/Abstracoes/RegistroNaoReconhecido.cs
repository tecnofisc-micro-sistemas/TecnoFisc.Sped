using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Registro emitido pelo leitor quando uma linha não pôde ser materializada como o tipo forte do
/// catálogo, em duas origens distintas — ambas preservam a linha crua completa e o
/// <see cref="ErroLayout"/> correspondente para o consumidor diagnosticar sem abortar o arquivo:
/// <list type="bullet">
///   <item>em modo <see cref="Parser.ReadingOptions.LenientLayout"/>, quando o código de registro é
///   desconhecido pelo catálogo;</item>
///   <item>em modo <see cref="Parser.ReadingOptions.RespeitarVigenciaDoLeiaute"/>, quando o registro
///   é conhecido pelo catálogo mas foi descartado por estar fora da versão declarada no <c>0000</c>
///   (<c>IntroduzidoEm</c> posterior à versão do arquivo) — aqui <see cref="Codigo"/> é o código
///   normativo do registro, não um código desconhecido.</item>
/// </list>
/// É sempre folha na hierarquia (nunca recebe filhos).
/// </summary>
public sealed class RegistroNaoReconhecido : RegistroSped
{
    public RegistroNaoReconhecido(string codigo, string linhaCrua, ErroLayout erro)
    {
        ArgumentNullException.ThrowIfNull(codigo);
        ArgumentNullException.ThrowIfNull(linhaCrua);
        ArgumentNullException.ThrowIfNull(erro);
        _codigo = codigo;
        LinhaCrua = linhaCrua;
        Erro = erro;
    }

    private readonly string _codigo;

    /// <summary>
    /// Código cru lido na posição 1 da linha. Desconhecido pelo catálogo na origem
    /// <see cref="Parser.ReadingOptions.LenientLayout"/>; código normativo válido (só fora de
    /// vigência) na origem <see cref="Parser.ReadingOptions.RespeitarVigenciaDoLeiaute"/> — ver
    /// <see cref="Erro"/> para distinguir as duas.
    /// </summary>
    public override string Codigo => _codigo;

    /// <summary>Linha SPED crua completa (com pipes), preservada verbatim.</summary>
    public string LinhaCrua { get; }

    /// <summary>Diagnóstico de layout associado.</summary>
    public ErroLayout Erro { get; }
}

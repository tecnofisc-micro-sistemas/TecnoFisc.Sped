using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Registro emitido pelo leitor em modo <see cref="Parser.ReadingOptions.LenientLayout"/> quando o
/// código de registro é desconhecido pelo catálogo. Preserva a linha crua completa e o
/// <see cref="ErroLayout"/> correspondente para o consumidor diagnosticar sem abortar o arquivo.
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

    /// <summary>Código cru lido na posição 1 da linha (desconhecido pelo catálogo).</summary>
    public override string Codigo => _codigo;

    /// <summary>Linha SPED crua completa (com pipes), preservada verbatim.</summary>
    public string LinhaCrua { get; }

    /// <summary>Diagnóstico de layout associado.</summary>
    public ErroLayout Erro { get; }
}

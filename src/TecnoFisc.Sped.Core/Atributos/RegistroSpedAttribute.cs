namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca uma classe como representante de um registro SPED. Os campos do registro são marcados
/// individualmente com <see cref="CampoSpedAttribute"/>. As propriedades do atributo descrevem
/// o registro no catálogo: código (ex.: "C100"), nível hierárquico (raiz = 0) e bloco.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RegistroSpedAttribute : Attribute
{
    /// <summary>Código do registro como aparece no arquivo SPED (ex.: "0000", "C100", "9999").</summary>
    public required string Codigo { get; init; }

    /// <summary>Nível hierárquico: 0 = raiz, 1 = abertura/fechamento de bloco, 2+ = detalhe.</summary>
    public required int Nivel { get; init; }

    /// <summary>Identificador do bloco ao qual o registro pertence (ex.: "0", "C", "9").</summary>
    public required string Bloco { get; init; }

    /// <summary>
    /// Versão do leiaute em que o registro foi introduzido. Convenção idêntica à de
    /// <see cref="CampoSpedAttribute.DesdeVersao"/>: valor numérico do enum <c>LayoutXxx</c>
    /// do módulo (ex.: <c>(int)LayoutEfdIcmsIpi.V015</c> = 15). <c>0</c> (default) significa
    /// "presente em todas as versões". O parser ignora o registro quando a versão do arquivo
    /// for menor; o gerador recusa emitir o registro nessa situação.
    /// </summary>
    public int IntroduzidoEm { get; init; }

    /// <summary>
    /// Token de fim de um registro com campo-arquivo embutido (ex.: <c>"J800FIM"</c>). Quando
    /// não nulo, o registro é <b>multi-linha</b>: seu conteúdo (tipicamente um arquivo RTF de até
    /// 30 MB no campo marcado com <see cref="CampoSpedAttribute.CampoArquivo"/>) carrega quebras
    /// de linha CRLF e ocupa várias linhas físicas no arquivo SPED. O leitor acumula linhas físicas
    /// a partir do início do registro até a linha que termina em <c>|{token}|</c>, preservando as
    /// quebras internas. <c>null</c> (default) = registro de uma linha física, comportamento padrão.
    /// </summary>
    public string? TokenFimArquivo { get; init; }
}

using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Catalogo;

/// <summary>
/// Descrição de um tipo de registro SPED conhecido pelo catálogo: código, posição na
/// hierarquia, fábrica para criar instâncias e a lista ordenada de campos.
/// </summary>
public sealed class MetadadosRegistro
{
    public MetadadosRegistro(
        string codigo,
        int nivel,
        string bloco,
        Type tipoCSharp,
        Func<RegistroSped> fabrica,
        IReadOnlyList<MetadadosCampo> campos,
        int introduzidoEm = 0,
        int descontinuadoEm = 0)
    {
        Codigo = codigo;
        Nivel = nivel;
        Bloco = bloco;
        TipoCSharp = tipoCSharp;
        Fabrica = fabrica;
        Campos = campos;
        IntroduzidoEm = introduzidoEm;
        DescontinuadoEm = descontinuadoEm;
    }

    public string Codigo { get; }
    public int Nivel { get; }
    public string Bloco { get; }
    public Type TipoCSharp { get; }
    public Func<RegistroSped> Fabrica { get; }

    /// <summary>Campos ordenados pela posição (1-based, sem incluir REG).</summary>
    public IReadOnlyList<MetadadosCampo> Campos { get; }

    /// <summary>
    /// Primeira versão de leiaute em que o registro passa a existir. <c>0</c> = sempre presente.
    /// Origem em <see cref="Atributos.RegistroSpedAttribute.IntroduzidoEm"/>.
    /// </summary>
    public int IntroduzidoEm { get; }

    /// <summary>
    /// Versão de leiaute a partir da qual o registro foi descontinuado. <c>0</c> = ativo em todas as versões.
    /// Origem em <see cref="Atributos.DescontinuadoAttribute.EmVersao"/>. Informacional — o parser
    /// continua reconhecendo o registro para suportar leitura de arquivos históricos das versões
    /// anteriores (ARCHITECTURE §4.7 read-only).
    /// </summary>
    public int DescontinuadoEm { get; }
}

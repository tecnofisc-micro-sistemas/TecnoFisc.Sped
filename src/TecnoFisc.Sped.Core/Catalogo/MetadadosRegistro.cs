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
        IReadOnlyList<MetadadosCampo> campos)
    {
        Codigo = codigo;
        Nivel = nivel;
        Bloco = bloco;
        TipoCSharp = tipoCSharp;
        Fabrica = fabrica;
        Campos = campos;
    }

    public string Codigo { get; }
    public int Nivel { get; }
    public string Bloco { get; }
    public Type TipoCSharp { get; }
    public Func<RegistroSped> Fabrica { get; }

    /// <summary>Campos ordenados pela posição (1-based, sem incluir REG).</summary>
    public IReadOnlyList<MetadadosCampo> Campos { get; }
}

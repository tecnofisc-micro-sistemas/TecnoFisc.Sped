using System.Diagnostics.CodeAnalysis;

using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Acesso por código aos metadados dos registros conhecidos por um projeto de formato.
/// Implementado pelo <see cref="CatalogoBuilder"/> via reflexão (Stage 2) e, no Stage 6,
/// por catálogos gerados em tempo de compilação (zero reflexão na hot path).
/// </summary>
public interface IRegistroSpedCatalogo
{
    /// <summary>Tenta resolver os metadados do registro pelo código (ex.: "C100").</summary>
    public bool TentarObter(ReadOnlySpan<char> codigo, [MaybeNullWhen(false)] out MetadadosRegistro metadados);

    /// <summary>Enumera todos os registros catalogados.</summary>
    public IEnumerable<MetadadosRegistro> EnumerarRegistros();
}

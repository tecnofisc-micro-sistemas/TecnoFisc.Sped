namespace TecnoFisc.Sped.Core.Abstracoes;

/// <summary>
/// Bloco SPED — agrupa registros que compartilham o mesmo identificador de bloco no layout.
/// Implementações concretas vivem em cada projeto de formato (EFD Contribuições, Fiscal, etc.).
/// </summary>
public interface IBlocoSped
{
    /// <summary>Identificador do bloco (ex.: "0", "C", "9").</summary>
    public string Identificador { get; }

    /// <summary>Enumera todos os registros do bloco em ordem de gravação.</summary>
    public IEnumerable<RegistroSped> EnumerarRegistros();
}

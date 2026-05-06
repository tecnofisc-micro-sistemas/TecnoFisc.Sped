using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.EfdContribuicoes;

/// <summary>
/// Bloco do leiaute EFD Contribuições. Carrega os registros de um bloco na ordem em que
/// devem aparecer no arquivo SPED, sem duplicar a tipagem por registro — quem precisa de
/// um registro específico filtra pela coleção <see cref="Registros"/>.
/// </summary>
/// <remarks>
/// O bloco não impõe regras de cardinalidade: aceita qualquer registro adicionado pelo
/// <see cref="ArquivoEfdContribuicoes"/>. Validações cross-record vivem no parser e nos
/// próprios registros (Stage 7+).
/// </remarks>
public sealed class BlocoEfdContribuicoes : IBlocoSped
{
    private readonly List<RegistroSped> _registros = [];

    internal BlocoEfdContribuicoes(string identificador)
    {
        Identificador = identificador;
    }

    /// <inheritdoc />
    public string Identificador { get; }

    /// <summary>Registros do bloco na ordem em que devem ser gravados.</summary>
    public IReadOnlyList<RegistroSped> Registros => _registros;

    internal void Adicionar(RegistroSped registro) => _registros.Add(registro);

    /// <inheritdoc />
    public IEnumerable<RegistroSped> EnumerarRegistros() => _registros;
}

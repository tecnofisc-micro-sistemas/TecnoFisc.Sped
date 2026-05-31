using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecd;

/// <summary>
/// Bloco do leiaute ECD. Carrega os registros de um bloco na ordem em que devem aparecer
/// no arquivo SPED.
/// </summary>
public sealed class BlocoEcd : IBlocoSped
{
    private readonly List<RegistroSped> _registros = [];

    internal BlocoEcd(string identificador)
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

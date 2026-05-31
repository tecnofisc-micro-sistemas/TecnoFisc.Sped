using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.EfdIcmsIpi;

/// <summary>
/// Bloco do leiaute EFD ICMS-IPI. Carrega os registros de um bloco na ordem em que
/// devem aparecer no arquivo SPED.
/// </summary>
public sealed class BlocoEfdIcmsIpi : IBlocoSped
{
    private readonly List<RegistroSped> _registros = [];

    internal BlocoEfdIcmsIpi(string identificador)
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

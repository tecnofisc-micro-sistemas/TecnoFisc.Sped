using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf;

/// <summary>Bloco do leiaute ECF com registros na ordem de leitura.</summary>
public sealed class BlocoEcf : IBlocoSped
{
    private readonly List<RegistroSped> _registros = [];

    internal BlocoEcf(string identificador)
    {
        Identificador = identificador;
    }

    /// <inheritdoc />
    public string Identificador { get; }

    /// <summary>Registros do bloco na ordem de leitura.</summary>
    public IReadOnlyList<RegistroSped> Registros => _registros;

    internal void Adicionar(RegistroSped registro) => _registros.Add(registro);

    /// <inheritdoc />
    public IEnumerable<RegistroSped> EnumerarRegistros() => _registros;
}

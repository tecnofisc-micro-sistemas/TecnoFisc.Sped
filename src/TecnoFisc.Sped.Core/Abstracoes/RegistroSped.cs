namespace TecnoFisc.Sped.Core.Abstracoes;

/// <summary>
/// Base abstrata de todos os registros SPED. Carrega apenas o vínculo hierárquico genérico
/// (Pai/Filhos); as colunas tipadas vivem nas subclasses concretas decoradas com
/// <see cref="Atributos.RegistroSpedAttribute"/> e <see cref="Atributos.CampoSpedAttribute"/>.
/// </summary>
public abstract class RegistroSped
{
    private readonly List<RegistroSped> _filhos = [];

    /// <summary>Código do registro como aparece no arquivo SPED (ex.: "0000", "C100").</summary>
    public abstract string Codigo { get; }

    /// <summary>Registro pai na hierarquia, ou <c>null</c> se este é o raiz.</summary>
    public RegistroSped? Pai { get; internal set; }

    /// <summary>Filhos diretos na hierarquia, na ordem de aparição no arquivo.</summary>
    public IReadOnlyList<RegistroSped> Filhos => _filhos;

    internal void AdicionarFilho(RegistroSped filho)
    {
        filho.Pai = this;
        _filhos.Add(filho);
    }
}

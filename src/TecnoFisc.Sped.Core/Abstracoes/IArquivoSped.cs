namespace TecnoFisc.Sped.Core.Abstracoes;

/// <summary>
/// Modelo de topo que representa um arquivo SPED completo. Cada projeto de formato expõe
/// uma classe concreta (ex.: ArquivoEfdContribuicoes) com coleções tipadas por bloco.
/// </summary>
public interface IArquivoSped
{
    /// <summary>Enumera os blocos que compõem o arquivo.</summary>
    public IEnumerable<IBlocoSped> EnumerarBlocos();
}

using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.EfdContribuicoes.Generated;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Compara a inicialização do catálogo de registros do leiaute EFD Contribuições entre as
/// duas estratégias suportadas pela biblioteca:
/// <list type="bullet">
///   <item>Catálogo reflexivo (Stage 2): <see cref="CatalogoBuilder.BuildFromAssembly"/> escaneia
///   o assembly inteiro com <c>Assembly.GetTypes()</c>, lê os atributos via reflexão e compila
///   delegates de setter/getter por <c>Expression.Compile</c>.</item>
///   <item>Catálogo gerado (Stage 6): <see cref="CatalogoSpedGerado"/> tem o dicionário
///   populado em compile-time via source generator. As fábricas são <c>static () =&gt; new T()</c>
///   diretas; só os metadados de campos ainda passam por reflexão one-time herdada do helper
///   compartilhado.</item>
/// </list>
/// <c>IterationSetup</c> chama <see cref="CatalogoBuilder.LimparCache"/> antes do benchmark
/// reflexivo para que o caminho não pegue cache de iterações anteriores e meça a varredura real.
/// </summary>
[MemoryDiagnoser]
public class InicializacaoCatalogoBenchmark
{
    [IterationSetup(Target = nameof(Reflexivo))]
    public void LimparCacheReflexivo() => CatalogoBuilder.LimparCache();

    [Benchmark(Baseline = true, Description = "Reflexivo (Assembly.GetTypes)")]
    public int Reflexivo()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(ParserEfdContribuicoes).Assembly);
        return catalogo.EnumerarRegistros().Count();
    }

    [Benchmark(Description = "Gerado (compile-time)")]
    public int Gerado()
    {
        var catalogo = new CatalogoSpedGerado();
        return catalogo.EnumerarRegistros().Count();
    }
}

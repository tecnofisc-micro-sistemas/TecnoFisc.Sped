using System.Runtime.CompilerServices;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Visitor;

/// <summary>
/// Smoke test do dispatcher emitido pelo source generator dentro do assembly
/// <c>TecnoFisc.Sped.EfdIcmsIpi</c>. Confirma que a interface e a extension
/// <c>DispatchAsync</c> são geradas e funcionam de ponta a ponta. Os testes
/// detalhados de comportamento ficam no assembly EFD Contribuições — aqui basta
/// garantir que a geração roda também neste projeto.
/// </summary>
public sealed class VisitorDispatcherSmokeTests
{
    private sealed class VisitorContador : IRegistroSpedVisitor
    {
        public int Visitas0000 { get; private set; }
        public int VisitasC100 { get; private set; }
        public int VisitasDesconhecidas { get; private set; }

        public ValueTask VisitAsync(Registro0000 registro, CancellationToken cancellationToken)
        {
            Visitas0000++;
            return default;
        }

        public ValueTask VisitAsync(RegistroC100 registro, CancellationToken cancellationToken)
        {
            VisitasC100++;
            return default;
        }

        public ValueTask VisitUnknownAsync(RegistroSped registro, CancellationToken cancellationToken)
        {
            VisitasDesconhecidas++;
            return default;
        }
    }

    [Fact]
    public async Task DispatchAsync_TiposEfdIcmsIpi_RoteiaCorretamente()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(new RegistroSped[]
        {
            new Registro0000(),
            new RegistroC100(),
            new RegistroC100(),
        }, ct);

        var visitor = new VisitorContador();
        await origem.DispatchAsync(visitor, ct);

        visitor.Visitas0000.Should().Be(1);
        visitor.VisitasC100.Should().Be(2);
        visitor.VisitasDesconhecidas.Should().Be(0);
    }

    private static async IAsyncEnumerable<T> AsAsync<T>(
        IReadOnlyList<T> items,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return item;
        }
    }
}

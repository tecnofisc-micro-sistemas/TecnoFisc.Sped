using System.Runtime.CompilerServices;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Visitor;

/// <summary>
/// Cobre o despachador <c>RegistroSpedVisitorExtensions.DispatchAsync</c> + interface
/// <c>IRegistroSpedVisitor</c> emitidos pelo source generator do Core. Garante que cada
/// overload <c>VisitAsync(Tipo)</c> é resolvido em compile-time e que registros fora do
/// assembly caem em <c>VisitUnknownAsync</c>.
/// </summary>
public sealed class VisitorDispatcherTests
{
    private sealed class RegistroForaDoAssembly : RegistroSped
    {
        public override string Codigo => "FORA";
    }

    private sealed class VisitorContador : IRegistroSpedVisitor
    {
        public int Visitas0000 { get; private set; }
        public int VisitasC100 { get; private set; }
        public int VisitasC170 { get; private set; }
        public int VisitasDesconhecidas { get; private set; }
        public List<string> Ordem { get; } = [];

        public ValueTask VisitAsync(Registro0000 registro, CancellationToken cancellationToken)
        {
            Visitas0000++;
            Ordem.Add("0000");
            return default;
        }

        public ValueTask VisitAsync(RegistroC100 registro, CancellationToken cancellationToken)
        {
            VisitasC100++;
            Ordem.Add("C100");
            return default;
        }

        public ValueTask VisitAsync(RegistroC170 registro, CancellationToken cancellationToken)
        {
            VisitasC170++;
            Ordem.Add("C170");
            return default;
        }

        public ValueTask VisitUnknownAsync(RegistroSped registro, CancellationToken cancellationToken)
        {
            VisitasDesconhecidas++;
            Ordem.Add("UNK:" + registro.Codigo);
            return default;
        }
    }

    [Fact]
    public async Task DispatchAsync_MisturaDeRegistros_RoteiaCadaTipoParaSeuOverload()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(new RegistroSped[]
        {
            new Registro0000(),
            new RegistroC100(),
            new RegistroC170(),
            new RegistroC100(),
            new RegistroC170(),
            new RegistroC170(),
        }, ct);

        var visitor = new VisitorContador();
        await origem.DispatchAsync(visitor, ct);

        visitor.Visitas0000.Should().Be(1);
        visitor.VisitasC100.Should().Be(2);
        visitor.VisitasC170.Should().Be(3);
        visitor.VisitasDesconhecidas.Should().Be(0);
        visitor.Ordem.Should().Equal("0000", "C100", "C170", "C100", "C170", "C170");
    }

    [Fact]
    public async Task DispatchAsync_RegistroDeOutroAssembly_CaiNoVisitUnknownAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(new RegistroSped[]
        {
            new Registro0000(),
            new RegistroForaDoAssembly(),
            new RegistroC100(),
        }, ct);

        var visitor = new VisitorContador();
        await origem.DispatchAsync(visitor, ct);

        visitor.Visitas0000.Should().Be(1);
        visitor.VisitasC100.Should().Be(1);
        visitor.VisitasDesconhecidas.Should().Be(1);
        visitor.Ordem.Should().Equal("0000", "UNK:FORA", "C100");
    }

    [Fact]
    public async Task DispatchAsync_VisitorSomenteOverrideUmTipo_OutrosUsamDefaultNaoOp()
    {
        var ct = TestContext.Current.CancellationToken;

        var origem = AsAsync(new RegistroSped[]
        {
            new Registro0000(),
            new RegistroC100(),
            new RegistroC170(),
        }, ct);

        var visitor = new VisitorMinimoSoZero();
        await origem.DispatchAsync(visitor, ct);

        visitor.Visitas0000.Should().Be(1);
    }

    private sealed class VisitorMinimoSoZero : IRegistroSpedVisitor
    {
        public int Visitas0000 { get; private set; }

        public ValueTask VisitAsync(Registro0000 registro, CancellationToken cancellationToken)
        {
            Visitas0000++;
            return default;
        }
    }

    [Fact]
    public async Task DispatchAsync_OrigemNula_LancaArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        IAsyncEnumerable<RegistroSped> origem = null!;
        var visitor = new VisitorContador();

        var act = async () => await origem.DispatchAsync(visitor, ct);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DispatchAsync_VisitorNulo_LancaArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(new RegistroSped[] { new Registro0000() }, ct);
        IRegistroSpedVisitor visitor = null!;

        var act = async () => await origem.DispatchAsync(visitor, ct);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DispatchAsync_CancelamentoSolicitado_PropagaOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        var origem = InfiniteAsync(cts.Token);

        var visitor = new VisitorContadorComCancelamento(cts);

        var act = async () => await origem.DispatchAsync(visitor, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class VisitorContadorComCancelamento(CancellationTokenSource cts) : IRegistroSpedVisitor
    {
        public ValueTask VisitAsync(Registro0000 registro, CancellationToken cancellationToken)
        {
            cts.Cancel();
            return default;
        }
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

    private static async IAsyncEnumerable<RegistroSped> InfiniteAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return new Registro0000();
        }
    }
}

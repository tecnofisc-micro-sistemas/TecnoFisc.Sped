using System.Runtime.CompilerServices;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Streaming;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Streaming;

public sealed class StreamingExtensionsTests
{
    private static readonly int[] SeqUmATres = [1, 2, 3];
    private static readonly int[] SeqUmAQuatro = [1, 2, 3, 4];
    private static readonly int[] SeqUmASeis = [1, 2, 3, 4, 5, 6];
    private static readonly int[] SeqUmASete = [1, 2, 3, 4, 5, 6, 7];

    private sealed class RegistroFakeA : RegistroSped
    {
        public override string Codigo => "FAKE_A";
    }

    private sealed class RegistroFakeB : RegistroSped
    {
        public override string Codigo => "FAKE_B";
    }

    [Fact]
    public async Task OfType_MisturaDeTipos_RetornaSomenteOTipoEsperado()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(new RegistroSped[]
        {
            new RegistroFakeA(),
            new RegistroFakeB(),
            new RegistroFakeA(),
            new RegistroFakeB(),
            new RegistroFakeA(),
        }, ct);

        var filtrados = new List<RegistroFakeA>();
        await foreach (var item in origem.OfType<RegistroFakeA>(ct))
        {
            filtrados.Add(item);
        }

        filtrados.Should().HaveCount(3);
        filtrados.Should().OnlyContain(r => r.Codigo == "FAKE_A");
    }

    [Fact]
    public async Task OfType_NenhumDoTipo_RetornaVazio()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync<RegistroSped>(new RegistroSped[] { new RegistroFakeB(), new RegistroFakeB() }, ct);

        var filtrados = new List<RegistroFakeA>();
        await foreach (var item in origem.OfType<RegistroFakeA>(ct))
        {
            filtrados.Add(item);
        }

        filtrados.Should().BeEmpty();
    }

    [Fact]
    public async Task OfType_OrigemNula_LancaArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        IAsyncEnumerable<RegistroSped> origem = null!;

        var act = async () =>
        {
            await foreach (var _ in origem.OfType<RegistroFakeA>(ct)) { }
        };

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task OfType_CancelamentoSolicitado_LancaOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        var origem = InfiniteAsync(cts.Token);

        var act = async () =>
        {
            await foreach (var _ in origem.OfType<RegistroFakeA>(cts.Token))
            {
                cts.Cancel();
            }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Batch_QuantidadeMultiplaDoTamanho_LotesCompletos()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(SeqUmASeis, ct);

        var lotes = new List<IReadOnlyList<int>>();
        await foreach (var lote in origem.Batch(2, ct))
        {
            lotes.Add(lote);
        }

        lotes.Should().HaveCount(3);
        lotes[0].Should().Equal(1, 2);
        lotes[1].Should().Equal(3, 4);
        lotes[2].Should().Equal(5, 6);
    }

    [Fact]
    public async Task Batch_QuantidadeNaoMultiplaDoTamanho_UltimoLoteParcial()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(SeqUmASete, ct);

        var lotes = new List<IReadOnlyList<int>>();
        await foreach (var lote in origem.Batch(3, ct))
        {
            lotes.Add(lote);
        }

        lotes.Should().HaveCount(3);
        lotes[0].Should().Equal(1, 2, 3);
        lotes[1].Should().Equal(4, 5, 6);
        lotes[2].Should().Equal(7);
    }

    [Fact]
    public async Task Batch_OrigemVazia_RetornaZeroLotes()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(Array.Empty<int>(), ct);

        var lotes = new List<IReadOnlyList<int>>();
        await foreach (var lote in origem.Batch(10, ct))
        {
            lotes.Add(lote);
        }

        lotes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Batch_TamanhoInvalido_LancaArgumentOutOfRangeException(int size)
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(SeqUmATres, ct);

        var act = async () =>
        {
            await foreach (var _ in origem.Batch(size, ct)) { }
        };

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Batch_OrigemNula_LancaArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        IAsyncEnumerable<int> origem = null!;

        var act = async () =>
        {
            await foreach (var _ in origem.Batch(10, ct)) { }
        };

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Batch_LotesNaoCompartilhamBufferInterno()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(SeqUmAQuatro, ct);

        var lotes = new List<IReadOnlyList<int>>();
        await foreach (var lote in origem.Batch(2, ct))
        {
            lotes.Add(lote);
        }

        lotes.Should().HaveCount(2);
        lotes[0].Should().Equal(1, 2);
        lotes[1].Should().Equal(3, 4);
    }

    [Fact]
    public async Task OfTypeMaisBatch_Composicao_AgrupaPorTipoFiltrado()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(new RegistroSped[]
        {
            new RegistroFakeA(),
            new RegistroFakeB(),
            new RegistroFakeA(),
            new RegistroFakeA(),
            new RegistroFakeB(),
            new RegistroFakeA(),
            new RegistroFakeA(),
        }, ct);

        var lotes = new List<IReadOnlyList<RegistroFakeA>>();
        await foreach (var lote in origem.OfType<RegistroFakeA>(ct).Batch(2, ct))
        {
            lotes.Add(lote);
        }

        lotes.Should().HaveCount(3);
        lotes[0].Should().HaveCount(2);
        lotes[1].Should().HaveCount(2);
        lotes[2].Should().HaveCount(1);
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
            yield return new RegistroFakeA();
        }
    }
}

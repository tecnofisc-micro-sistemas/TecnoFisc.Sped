using System.Runtime.CompilerServices;
using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Core.Streaming;

namespace TecnoFisc.Sped.Core.Tests.Streaming;

public sealed class WithContextTests
{
    private sealed class RegistroFake(string codigo) : RegistroSped
    {
        public override string Codigo { get; } = codigo;

        public static RegistroFake Filho(RegistroFake pai, string codigo)
        {
            var f = new RegistroFake(codigo);
            pai.GetType().GetMethod("AdicionarFilho",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(pai, [f]);
            return f;
        }
    }

    [Fact]
    public async Task WithContext_StreamComUmRaiz_IdPaiEhNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var raiz = new RegistroFake("0000");
        var origem = AsAsync(new RegistroSped[] { raiz }, ct);

        var coletados = new List<(RegistroSped Reg, ContextoPersistencia Ctx)>();
        await foreach (var item in origem.WithContext(cancellationToken: ct))
        {
            coletados.Add(item);
        }

        coletados.Should().HaveCount(1);
        coletados[0].Reg.Should().BeSameAs(raiz);
        coletados[0].Ctx.IdRegistroAtual.Should().Be(1);
        coletados[0].Ctx.IdPai.Should().BeNull();
    }

    [Fact]
    public async Task WithContext_FilhoDeRaiz_IdPaiEhDoRaiz()
    {
        var ct = TestContext.Current.CancellationToken;
        var raiz = new RegistroFake("0000");
        var filho = RegistroFake.Filho(raiz, "0001");
        var origem = AsAsync(new RegistroSped[] { raiz, filho }, ct);

        var coletados = new List<ContextoPersistencia>();
        await foreach (var (_, ctx) in origem.WithContext(cancellationToken: ct))
        {
            coletados.Add(ctx);
        }

        coletados.Should().HaveCount(2);
        coletados[0].IdRegistroAtual.Should().Be(1);
        coletados[0].IdPai.Should().BeNull();
        coletados[1].IdRegistroAtual.Should().Be(2);
        coletados[1].IdPai.Should().Be(1);
    }

    [Fact]
    public async Task WithContext_TresNiveis_CadaFilhoApontaParaPaiImediato()
    {
        var ct = TestContext.Current.CancellationToken;
        var raiz = new RegistroFake("0000");
        var bloco = RegistroFake.Filho(raiz, "C001");
        var doc = RegistroFake.Filho(bloco, "C100");
        var item = RegistroFake.Filho(doc, "C170");
        var origem = AsAsync(new RegistroSped[] { raiz, bloco, doc, item }, ct);

        var coletados = new List<ContextoPersistencia>();
        await foreach (var (_, ctx) in origem.WithContext(cancellationToken: ct))
        {
            coletados.Add(ctx);
        }

        coletados.Should().HaveCount(4);
        coletados[0].IdRegistroAtual.Should().Be(1);
        coletados[0].IdPai.Should().BeNull();
        coletados[1].IdRegistroAtual.Should().Be(2);
        coletados[1].IdPai.Should().Be(1);
        coletados[2].IdRegistroAtual.Should().Be(3);
        coletados[2].IdPai.Should().Be(2);
        coletados[3].IdRegistroAtual.Should().Be(4);
        coletados[3].IdPai.Should().Be(3);
    }

    [Fact]
    public async Task WithContext_HierarquiaRealComPilha_IdsPaiCorretos()
    {
        var ct = TestContext.Current.CancellationToken;

        // Cenário: 0000 → C001 → {C100 → C170, C170} → 9999
        var pilha = new PilhaHierarquica();
        var r0000 = new RegistroFake("0000");
        var c001 = new RegistroFake("C001");
        var c100 = new RegistroFake("C100");
        var c170a = new RegistroFake("C170");
        var c170b = new RegistroFake("C170");
        var r9999 = new RegistroFake("9999");

        VincularViaPilha(pilha, r0000, 0);
        VincularViaPilha(pilha, c001, 1);
        VincularViaPilha(pilha, c100, 2);
        VincularViaPilha(pilha, c170a, 3);
        VincularViaPilha(pilha, c170b, 3);
        VincularViaPilha(pilha, r9999, 0);

        var origem = AsAsync(new RegistroSped[] { r0000, c001, c100, c170a, c170b, r9999 }, ct);

        var coletados = new List<(string Codigo, long Id, long? IdPai)>();
        await foreach (var (reg, ctx) in origem.WithContext(cancellationToken: ct))
        {
            coletados.Add((reg.Codigo, ctx.IdRegistroAtual, ctx.IdPai));
        }

        coletados.Should().Equal(
            ("0000", 1L, null as long?),
            ("C001", 2L, 1L),
            ("C100", 3L, 2L),
            ("C170", 4L, 3L),
            ("C170", 5L, 3L),
            ("9999", 6L, null as long?));
    }

    [Fact]
    public async Task WithContext_StartAtCustomizado_RespeitaSeedInicial()
    {
        var ct = TestContext.Current.CancellationToken;
        var raiz = new RegistroFake("0000");
        var filho = RegistroFake.Filho(raiz, "0001");
        var origem = AsAsync(new RegistroSped[] { raiz, filho }, ct);

        var coletados = new List<ContextoPersistencia>();
        await foreach (var (_, ctx) in origem.WithContext(startAt: 1000, cancellationToken: ct))
        {
            coletados.Add(ctx);
        }

        coletados[0].IdRegistroAtual.Should().Be(1000);
        coletados[0].IdPai.Should().BeNull();
        coletados[1].IdRegistroAtual.Should().Be(1001);
        coletados[1].IdPai.Should().Be(1000);
    }

    [Fact]
    public async Task WithContext_OrigemNula_LancaArgumentNullException()
    {
        var ct = TestContext.Current.CancellationToken;
        IAsyncEnumerable<RegistroSped> origem = null!;

        var act = async () =>
        {
            await foreach (var _ in origem.WithContext(cancellationToken: ct)) { }
        };

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WithContext_StreamVazio_NaoEmiteNada()
    {
        var ct = TestContext.Current.CancellationToken;
        var origem = AsAsync(Array.Empty<RegistroSped>(), ct);

        var contagem = 0;
        await foreach (var _ in origem.WithContext(cancellationToken: ct))
        {
            contagem++;
        }

        contagem.Should().Be(0);
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

    private static void VincularViaPilha(PilhaHierarquica pilha, RegistroFake registro, int nivel)
    {
        var pai = pilha.Empilhar(registro, nivel);
        if (pai is not null)
        {
            pai.GetType().GetMethod("AdicionarFilho",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(pai, [registro]);
        }
    }
}

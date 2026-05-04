using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.Core.Tests.Parser;

public sealed class PilhaHierarquicaTests
{
    private sealed class RegistroFake(string codigo) : RegistroSped
    {
        public override string Codigo { get; } = codigo;
    }

    [Fact]
    public void Empilhar_PrimeiroRegistroNivel0_NaoTemPai()
    {
        var pilha = new PilhaHierarquica();

        var pai = pilha.Empilhar(new RegistroFake("0000"), nivel: 0);

        pai.Should().BeNull();
        pilha.Profundidade.Should().Be(1);
    }

    [Fact]
    public void Empilhar_FilhoDeNivelMaior_VinculaAoTopo()
    {
        var pilha = new PilhaHierarquica();
        var raiz = new RegistroFake("0000");
        var bloco = new RegistroFake("C001");

        pilha.Empilhar(raiz, 0);
        var pai = pilha.Empilhar(bloco, 1);

        pai.Should().BeSameAs(raiz);
    }

    [Fact]
    public void Empilhar_NivelIgualOuMenor_DesempilhaIrmaosFechados()
    {
        var pilha = new PilhaHierarquica();
        var r0000 = new RegistroFake("0000");
        var c001 = new RegistroFake("C001");
        var c100A = new RegistroFake("C100");
        var c170 = new RegistroFake("C170");
        var c100B = new RegistroFake("C100");

        pilha.Empilhar(r0000, 0);
        pilha.Empilhar(c001, 1);
        pilha.Empilhar(c100A, 2);
        pilha.Empilhar(c170, 3);

        // Novo C100 (nível 2) fecha c170 e c100A; pai deve ser c001.
        var pai = pilha.Empilhar(c100B, 2);

        pai.Should().BeSameAs(c001);
        pilha.Profundidade.Should().Be(3); // 0000, C001, C100b
        pilha.Topo.Should().BeSameAs(c100B);
    }

    [Fact]
    public void Empilhar_NivelZeroSubsequente_FechaTudo()
    {
        var pilha = new PilhaHierarquica();
        pilha.Empilhar(new RegistroFake("0000"), 0);
        pilha.Empilhar(new RegistroFake("C001"), 1);
        pilha.Empilhar(new RegistroFake("C100"), 2);

        var r9999 = new RegistroFake("9999");
        var pai = pilha.Empilhar(r9999, 0);

        pai.Should().BeNull();
        pilha.Profundidade.Should().Be(1);
        pilha.Topo.Should().BeSameAs(r9999);
    }

    [Fact]
    public void Limpar_RedefineParaPilhaVazia()
    {
        var pilha = new PilhaHierarquica();
        pilha.Empilhar(new RegistroFake("0000"), 0);
        pilha.Empilhar(new RegistroFake("C001"), 1);

        pilha.Limpar();

        pilha.Profundidade.Should().Be(0);
        pilha.Topo.Should().BeNull();
    }

    [Fact]
    public void Empilhar_RegistroNulo_LancaArgumentNullException()
    {
        var pilha = new PilhaHierarquica();

        var act = () => pilha.Empilhar(null!, 0);

        act.Should().Throw<ArgumentNullException>();
    }
}

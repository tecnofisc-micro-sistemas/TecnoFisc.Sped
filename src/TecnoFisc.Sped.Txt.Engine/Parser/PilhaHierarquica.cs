using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Mantém a pilha de registros já lidos para resolver o pai correto de cada novo registro.
/// Regra: o pai de um registro de nível <c>N</c> é o registro de nível <c>&lt; N</c> mais
/// próximo no topo da pilha; ao empilhar, qualquer registro de nível <c>&gt;= N</c> é
/// removido (encerrou-se o escopo dele). O nível 0 (raiz) é sempre o ancestral comum.
/// </summary>
public sealed class PilhaHierarquica
{
    private readonly Stack<Quadro> _pilha = new();

    private readonly record struct Quadro(RegistroSped Registro, int Nivel);

    /// <summary>Empilha um registro e devolve o pai resolvido (ou <c>null</c> para raiz).</summary>
    public RegistroSped? Empilhar(RegistroSped registro, int nivel)
    {
        ArgumentNullException.ThrowIfNull(registro);

        while (_pilha.TryPeek(out var topo) && topo.Nivel >= nivel)
            _pilha.Pop();

        RegistroSped? pai = _pilha.TryPeek(out var atual) ? atual.Registro : null;

        _pilha.Push(new Quadro(registro, nivel));
        return pai;
    }

    /// <summary>Limpa a pilha — usar entre arquivos quando o leitor é reaproveitado.</summary>
    public void Limpar() => _pilha.Clear();

    /// <summary>Topo atual, ou <c>null</c> se vazia.</summary>
    public RegistroSped? Topo => _pilha.TryPeek(out var quadro) ? quadro.Registro : null;

    /// <summary>Profundidade atual (para diagnóstico em testes).</summary>
    public int Profundidade => _pilha.Count;
}

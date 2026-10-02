using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Xml.Engine;

namespace TecnoFisc.Sped.NFeNFCe;

public abstract record Evento : IDocumentoFiscalXml
{
    public required ChaveAcesso ChaveAcesso { get; init; }
    public required int TpEvento { get; init; }
    public int? NSeqEvento { get; init; }
    public DateTimeOffset? DhEvento { get; init; }
    public string? Id { get; init; }
    public string? Versao { get; init; }
    public string? VersaoDetalhes { get; init; }
    public string? VerEvento { get; init; }
    public int? COrgao { get; init; }
    public int? TpAmb { get; init; }
    public string? CNPJ { get; init; }
    public string? CPF { get; init; }
    public string? DescEvento { get; init; }
    public RetornoEvento? Retorno { get; init; }
    /// <summary>Identidade completa usa chave/tipo/sequência; resumos sem sequência têm identidade por referência.</summary>
    public virtual bool Equals(Evento? other) => ReferenceEquals(this, other)
        || other is not null && EqualityContract == other.EqualityContract && NSeqEvento is not null
        && ChaveAcesso != default && ChaveAcesso == other.ChaveAcesso
        && TpEvento == other.TpEvento && NSeqEvento == other.NSeqEvento;
    public override int GetHashCode() => NSeqEvento is not null && ChaveAcesso != default
        ? HashCode.Combine(EqualityContract, ChaveAcesso, TpEvento, NSeqEvento)
        : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}

public sealed record EventoCancelamento : Evento
{
    public string? NProt { get; init; }
    public string? XJust { get; init; }
    /// <summary>Retorno vinculado à nota (135) ou cancelamento homologado fora de prazo (155).</summary>
    public bool IsEffective => NSeqEvento is not null && Retorno is { CStat: 135 or 155, TpEvento: 110111 }
        && Retorno.ChaveAcesso == ChaveAcesso && Retorno.NSeqEvento == NSeqEvento;
    public bool Equals(EventoCancelamento? other) => base.Equals(other);
    public override int GetHashCode() => base.GetHashCode();
}

public sealed record EventoGenerico : Evento
{
    public ElementoEvento? Detalhes { get; init; }
    public bool Equals(EventoGenerico? other) => base.Equals(other);
    public override int GetHashCode() => base.GetHashCode();
}

/// <summary>Detalhes de um evento não tipado; limitado pelo orçamento de bytes/profundidade do XML.</summary>
public sealed record ElementoEvento(string Nome, string Namespace, string? Texto,
    IReadOnlyList<AtributoEvento> Atributos, IReadOnlyList<ElementoEvento> Filhos);
public sealed record AtributoEvento(string Nome, string Namespace, string Valor);

public sealed record RetornoEvento
{
    public string? Versao { get; init; }
    public ChaveAcesso? ChaveAcesso { get; init; }
    public int? TpEvento { get; init; }
    public int? NSeqEvento { get; init; }
    public int? CStat { get; init; }
    public string? XMotivo { get; init; }
    public string? NProt { get; init; }
    public DateTimeOffset? DhRegEvento { get; init; }
    public string? Id { get; init; }
    public string? VerAplic { get; init; }
    public int? TpAmb { get; init; }
    public int? COrgao { get; init; }
    public string? XEvento { get; init; }
    public string? CNPJDest { get; init; }
    public string? CPFDest { get; init; }
    public string? EmailDest { get; init; }
}

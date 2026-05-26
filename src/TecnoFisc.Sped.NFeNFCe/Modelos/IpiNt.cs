namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>IPINT</c> — IPI não tributado (CST 01–05 ou 51–55).
/// Contém apenas o <c>CST</c> herdado da base; não há campos de valor.
/// </summary>
public sealed record IpiNt : IpiTributacao;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>COFINSNT</c> — COFINS não tributado
/// (CST 04–09: monofásico alíquota zero, ST, alíquota zero, isento, sem incidência, suspenso).
/// Contém apenas o <c>CST</c> herdado da base; não há campos de valor.
/// </summary>
public sealed record CofinsNt : Cofins;

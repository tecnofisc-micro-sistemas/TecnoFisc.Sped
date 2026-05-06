namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da utilização dos bens incorporados ao ativo imobilizado —
/// campo IND_UTIL_BEM_IMOB dos Registros F120 e F130.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 237 e 242.
/// </summary>
public enum IndicadorUtilizacaoBemImobilizado
{
    /// <summary>1 — Produção de Bens Destinados a Venda.</summary>
    ProducaoBensParaVenda = 1,

    /// <summary>2 — Prestação de Serviços.</summary>
    PrestacaoServicos = 2,

    /// <summary>3 — Locação a Terceiros.</summary>
    LocacaoTerceiros = 3,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}

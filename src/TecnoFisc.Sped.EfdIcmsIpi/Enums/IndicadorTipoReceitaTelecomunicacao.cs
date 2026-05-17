namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de receita — campo IND_REC do Registro D510 (itens de NF de Serviço de
/// Comunicação cód. 21 e Serviço de Telecomunicação cód. 22).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 194.
/// </summary>
public enum IndicadorTipoReceitaTelecomunicacao
{
    /// <summary>0 — Receita própria — serviços prestados.</summary>
    ReceitaPropriaServicosPrestados = 0,

    /// <summary>1 — Receita própria — cobrança de débitos.</summary>
    ReceitaPropriaCobrancaDebitos = 1,

    /// <summary>2 — Receita própria — venda de mercadorias.</summary>
    ReceitaPropriaVendaMercadorias = 2,

    /// <summary>3 — Receita própria — venda de serviço pré-pago.</summary>
    ReceitaPropriaVendaServicoPrepago = 3,

    /// <summary>4 — Outras receitas próprias.</summary>
    OutrasReceitasProprias = 4,

    /// <summary>5 — Receitas de terceiros (co-faturamento).</summary>
    ReceitasTerceirosCofaturamento = 5,

    /// <summary>9 — Outras receitas de terceiros.</summary>
    OutrasReceitasTerceiros = 9,
}

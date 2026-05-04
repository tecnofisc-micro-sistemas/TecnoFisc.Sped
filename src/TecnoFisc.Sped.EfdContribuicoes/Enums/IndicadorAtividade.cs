namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de atividade preponderante — campo IND_ATIV do Registro 0000. Valores
/// válidos pelo Guia Prático v1.35: <c>0, 1, 2, 3, 4, 9</c>. Note que <c>5..8</c> não são
/// definidos pelo layout.
/// </summary>
public enum IndicadorAtividade
{
    /// <summary>0 — Industrial ou equiparado a industrial.</summary>
    Industrial = 0,

    /// <summary>1 — Prestador de serviços.</summary>
    PrestadorServicos = 1,

    /// <summary>2 — Atividade de comércio.</summary>
    Comercio = 2,

    /// <summary>3 — Pessoa jurídica referida nos §§ 6º, 8º e 9º do art. 3º da Lei nº 9.718, de 1998.</summary>
    InstituicaoFinanceiraEAssemelhada = 3,

    /// <summary>4 — Atividade imobiliária.</summary>
    AtividadeImobiliaria = 4,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}

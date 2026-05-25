namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Código de relacionamento do participante — campo COD_REL do Registro 0180 da ECD.
/// Conforme Tabela de Códigos de Participação do Participante publicada pelo Sped.
/// Largura SPED: dois caracteres (Tam=002); o serializador zero-pad o underlying int.
/// </summary>
public enum CodigoRelacionamentoParticipante
{
    /// <summary>01 — Matriz no exterior.</summary>
    MatrizExterior = 1,

    /// <summary>02 — Filial, inclusive agência ou dependência, no exterior.</summary>
    FilialExterior = 2,

    /// <summary>03 — Coligada, inclusive equiparada.</summary>
    Coligada = 3,

    /// <summary>04 — Controladora.</summary>
    Controladora = 4,

    /// <summary>05 — Controlada (exceto subsidiária integral).</summary>
    Controlada = 5,

    /// <summary>06 — Subsidiária integral.</summary>
    SubsidiariaIntegral = 6,

    /// <summary>07 — Controlada em conjunto.</summary>
    ControladaEmConjunto = 7,

    /// <summary>08 — Entidade de Propósito Específico (conforme definição da CVM).</summary>
    EntidadePropositoEspecifico = 8,

    /// <summary>09 — Participante do conglomerado, conforme norma específica do órgão regulador, exceto as que se enquadrem nos tipos precedentes.</summary>
    ParticipanteConglomerado = 9,

    /// <summary>10 — Vinculadas (Art. 23 da Lei 9.430/96), exceto as que se enquadrem nos tipos precedentes.</summary>
    Vinculada = 10,

    /// <summary>11 — Localizada em país com tributação favorecida (Art. 24 da Lei 9.430/96), exceto as que se enquadrem nos tipos precedentes.</summary>
    PaisTributacaoFavorecida = 11,
}

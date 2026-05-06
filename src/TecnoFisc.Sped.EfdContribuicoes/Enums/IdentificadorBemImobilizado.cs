namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Identificação dos bens ou grupo de bens incorporados ao Ativo Imobilizado —
/// campo IDENT_BEM_IMOB dos Registros F120 e F130.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 237 e 242.
/// A largura SPED é fixa em dois caracteres (Tam=002*), de modo que o serializador
/// zero-pad o underlying int.
/// </summary>
public enum IdentificadorBemImobilizado
{
    /// <summary>01 — Edificações e Benfeitorias em Imóveis Próprios.</summary>
    EdificacoesBenfeitoriasImovelProprio = 1,

    /// <summary>02 — Edificações e Benfeitorias em Imóveis de Terceiros.</summary>
    EdificacoesBenfeitoriasImovelTerceiros = 2,

    /// <summary>03 — Instalações.</summary>
    Instalacoes = 3,

    /// <summary>04 — Máquinas.</summary>
    Maquinas = 4,

    /// <summary>05 — Equipamentos.</summary>
    Equipamentos = 5,

    /// <summary>06 — Veículos.</summary>
    Veiculos = 6,

    /// <summary>99 — Outros.</summary>
    Outros = 99,
}

using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Tipo de documento anexado como arquivo RTF — campo <c>TIPO_DOC</c> do Registro J800 da ECD.
/// 001 = Demonstração do Resultado Abrangente do Período;
/// 002 = Demonstração dos Fluxos de Caixa;
/// 003 = Demonstração do Valor Adicionado;
/// 010 = Notas Explicativas;
/// 011 = Relatório da Administração;
/// 012 = Parecer dos Auditores;
/// 099 = Outros.
/// </summary>
public enum TipoDocumentoJ800
{
    /// <summary>001 — Demonstração do Resultado Abrangente do Período.</summary>
    [SpedValor("001")]
    DemonstracaoResultadoAbrangente = 0,

    /// <summary>002 — Demonstração dos Fluxos de Caixa.</summary>
    [SpedValor("002")]
    DemonstracaoFluxosCaixa = 1,

    /// <summary>003 — Demonstração do Valor Adicionado.</summary>
    [SpedValor("003")]
    DemonstracaoValorAdicionado = 2,

    /// <summary>010 — Notas Explicativas.</summary>
    [SpedValor("010")]
    NotasExplicativas = 3,

    /// <summary>011 — Relatório da Administração.</summary>
    [SpedValor("011")]
    RelatorioAdministracao = 4,

    /// <summary>012 — Parecer dos Auditores.</summary>
    [SpedValor("012")]
    ParecerAuditores = 5,

    /// <summary>099 — Outros.</summary>
    [SpedValor("099")]
    Outros = 6,
}

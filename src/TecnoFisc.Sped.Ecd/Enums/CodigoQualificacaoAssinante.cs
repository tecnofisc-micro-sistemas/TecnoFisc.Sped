using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Código de qualificação do assinante da ECD — campos <c>COD_ASSIN</c> do Registro J930 e
/// <c>COD_ASSIN_T</c> do Registro J932. Identifica o papel/função do signatário.
/// Conforme Tabela de Qualificação do Assinante no Manual de Orientação do Leiaute 9 da ECD, p. 200–202.
/// </summary>
public enum CodigoQualificacaoAssinante
{
    /// <summary>001 — Pessoa Jurídica (e-CNPJ ou e-PJ). Exclusivo para assinatura por e-CNPJ/e-PJ.</summary>
    [SpedValor("001")]
    PessoaJuridica = 0,

    /// <summary>203 — Diretor.</summary>
    [SpedValor("203")]
    Diretor = 1,

    /// <summary>204 — Conselheiro de Administração.</summary>
    [SpedValor("204")]
    ConselheiroDeAdministracao = 2,

    /// <summary>205 — Administrador.</summary>
    [SpedValor("205")]
    Administrador = 3,

    /// <summary>206 — Administrador do Grupo.</summary>
    [SpedValor("206")]
    AdministradorDoGrupo = 4,

    /// <summary>207 — Administrador de Sociedade Filiada.</summary>
    [SpedValor("207")]
    AdministradorDeSociedadeFiliada = 5,

    /// <summary>220 — Administrador Judicial – Pessoa Física.</summary>
    [SpedValor("220")]
    AdministradorJudicialPessoaFisica = 6,

    /// <summary>222 — Administrador Judicial – Pessoa Jurídica - Profissional Responsável.</summary>
    [SpedValor("222")]
    AdministradorJudicialPessoaJuridicaProfissionalResponsavel = 7,

    /// <summary>223 — Administrador Judicial/Gestor.</summary>
    [SpedValor("223")]
    AdministradorJudicialGestor = 8,

    /// <summary>226 — Gestor Judicial.</summary>
    [SpedValor("226")]
    GestorJudicial = 9,

    /// <summary>309 — Procurador.</summary>
    [SpedValor("309")]
    Procurador = 10,

    /// <summary>312 — Inventariante.</summary>
    [SpedValor("312")]
    Inventariante = 11,

    /// <summary>313 — Liquidante.</summary>
    [SpedValor("313")]
    Liquidante = 12,

    /// <summary>315 — Interventor.</summary>
    [SpedValor("315")]
    Interventor = 13,

    /// <summary>401 — Titular – Pessoa Física - EIRELI.</summary>
    [SpedValor("401")]
    TitularPessoaFisicaEireli = 14,

    /// <summary>801 — Empresário.</summary>
    [SpedValor("801")]
    Empresario = 15,

    /// <summary>900 — Contador/Contabilista. Toda ECD deve ter ao menos um signatário com este código.</summary>
    [SpedValor("900")]
    ContadorContabilista = 16,

    /// <summary>940 — Auditor Independente.</summary>
    [SpedValor("940")]
    AuditorIndependente = 17,

    /// <summary>999 — Outros.</summary>
    [SpedValor("999")]
    Outros = 18,
}

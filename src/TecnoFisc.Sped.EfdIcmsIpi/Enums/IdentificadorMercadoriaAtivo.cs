namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Identificação do tipo de mercadoria no Cadastro de Bens ou Componentes do Ativo Imobilizado
/// — campo <c>IDENT_MERC</c> do Registro 0300 no EFD ICMS-IPI. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 39.
/// </summary>
public enum IdentificadorMercadoriaAtivo
{
    /// <summary>1 — Bem: mercadoria com todas as condições necessárias para ser utilizada nas atividades do estabelecimento.</summary>
    Bem = 1,

    /// <summary>2 — Componente: mercadoria que faz parte de um bem móvel em construção no estabelecimento.</summary>
    Componente = 2,
}

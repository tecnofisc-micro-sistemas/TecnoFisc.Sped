namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza da pessoa jurídica — campo IND_NAT_PJ do Registro 0000. Valores
/// válidos pelo Guia Prático v1.35: <c>00..05</c>. A largura SPED do campo é fixa em dois
/// caracteres (Tam=2*), de modo que o serializador zero-pad o underlying.
/// </summary>
public enum IndicadorNaturezaPessoaJuridica
{
    /// <summary>00 — Pessoa jurídica em geral (não participante de SCP como sócia ostensiva).</summary>
    PessoaJuridicaEmGeral = 0,

    /// <summary>01 — Sociedade cooperativa (não participante de SCP como sócia ostensiva).</summary>
    SociedadeCooperativa = 1,

    /// <summary>02 — Entidade sujeita ao PIS/Pasep exclusivamente com base na Folha de Salários.</summary>
    EntidadeFolhaSalarios = 2,

    /// <summary>03 — Pessoa jurídica em geral participante de SCP como sócia ostensiva.</summary>
    PessoaJuridicaEmGeralSociaSCP = 3,

    /// <summary>04 — Sociedade cooperativa participante de SCP como sócia ostensiva.</summary>
    SociedadeCooperativaSociaSCP = 4,

    /// <summary>05 — Sociedade em Conta de Participação - SCP.</summary>
    SociedadeContaParticipacao = 5,
}

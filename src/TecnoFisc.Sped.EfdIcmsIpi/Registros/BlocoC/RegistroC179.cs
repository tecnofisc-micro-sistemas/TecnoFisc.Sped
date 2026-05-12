using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C179 — Complemento de Item: Informações Complementares ST (cód. 01).
/// Informa repasse, dedução e complemento de ICMS ST em operações interestaduais
/// e com substituto intermediário.
/// Nível hierárquico 4, ocorrência 1:1 (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 90.
/// </summary>
[RegistroSped(Codigo = "C179", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC179 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C179";

    /// <summary>Valor da base de cálculo ST na origem/destino em operações interestaduais.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? BcStOrigDest { get; set; }

    /// <summary>Valor do ICMS ST a repassar/deduzir em operações interestaduais.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? IcmsStRep { get; set; }

    /// <summary>Valor do ICMS ST a complementar à UF de destino.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? IcmsStCompl { get; set; }

    /// <summary>Valor da BC de retenção em remessa promovida por Substituído intermediário.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? BcRet { get; set; }

    /// <summary>Valor da parcela do imposto retido em remessa promovida por substituído intermediário. Pode ser negativo (N').</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? IcmsRet { get; set; }
}

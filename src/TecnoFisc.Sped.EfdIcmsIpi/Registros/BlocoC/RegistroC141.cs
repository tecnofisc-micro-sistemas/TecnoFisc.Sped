using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C141 — Complemento — Vencimento da Fatura (cód. 01).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 74.
/// </summary>
[RegistroSped(Codigo = "C141", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC141 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C141";

    /// <summary>Número da parcela a receber/pagar.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int NumParc { get; set; }

    /// <summary>Data de vencimento da parcela no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtVcto { get; set; }

    /// <summary>Valor da parcela a receber/pagar.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlParc { get; set; }
}

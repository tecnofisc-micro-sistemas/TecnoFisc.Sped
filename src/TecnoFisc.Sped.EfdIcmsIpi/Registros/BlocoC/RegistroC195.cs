using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C195 — Complemento do Registro Analítico — Observações do Lançamento Fiscal
/// (cód. 01, 1B, 04, 55 e 65). Registra observações decorrentes da legislação estadual
/// relativas a ajustes, diferencial de alíquota, antecipação de imposto e situações afins.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 105.
/// </summary>
[RegistroSped(Codigo = "C195", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC195 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C195";

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodObs { get; set; }

    /// <summary>Descrição complementar do código de observação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}

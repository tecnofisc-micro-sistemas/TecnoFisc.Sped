using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C595 — Observações do Lançamento Fiscal (cód. 06, 28, 29 e 66).
/// Registra observações decorrentes da legislação estadual relativas a ajustes,
/// diferencial de alíquota, antecipação de imposto e situações afins, vinculadas
/// às NF/Conta de Energia Elétrica, NF3e, NF/Conta D'Água e NF Gás.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 145.
/// </summary>
[RegistroSped(Codigo = "C595", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC595 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C595";

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodObs { get; set; }

    /// <summary>Descrição complementar do código de observação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}

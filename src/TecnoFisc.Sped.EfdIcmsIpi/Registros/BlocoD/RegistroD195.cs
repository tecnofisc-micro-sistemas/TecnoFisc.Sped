using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D195 — Observações do Lançamento Fiscal (cód. 07, 08, 8B, 09, 10, 11, 26, 27, 57, 63 e 67).
/// Informado quando houver ajustes nos documentos fiscais decorrentes de legislação estadual.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 179.
/// </summary>
[RegistroSped(Codigo = "D195", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD195 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D195";

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? CodObs { get; set; }

    /// <summary>Descrição complementar do código de observação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}

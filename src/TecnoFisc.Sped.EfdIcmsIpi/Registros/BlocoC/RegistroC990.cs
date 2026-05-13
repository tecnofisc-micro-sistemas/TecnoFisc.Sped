using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C990 — Encerramento do Bloco C. Nível hierárquico 1, ocorrência única por
/// arquivo. O campo QTD_LIN_C é populado pelo totalizador de blocos (Stage 3) — aqui
/// declara apenas o layout. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 163.
/// </summary>
[RegistroSped(Codigo = "C990", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C990";

    /// <summary>Quantidade total de linhas do Bloco C, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinC { get; set; }
}

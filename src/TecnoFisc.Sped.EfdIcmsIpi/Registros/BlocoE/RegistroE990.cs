using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E990 — Encerramento do Bloco E. Nível hierárquico 1, ocorrência única por
/// arquivo. O campo QTD_LIN_E é populado pelo totalizador de blocos (Stage 3) — aqui
/// declara apenas o layout. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 238.
/// </summary>
[RegistroSped(Codigo = "E990", Nivel = 1, Bloco = "E")]
public sealed partial class RegistroE990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E990";

    /// <summary>Quantidade total de linhas do Bloco E, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinE { get; set; }
}

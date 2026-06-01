using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B990 — Encerramento do Bloco B. Nível hierárquico 1, ocorrência única por
/// arquivo. O campo QTD_LIN_B é populado pelo totalizador de blocos (Stage 3) — aqui
/// declara apenas o layout. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 58.
/// </summary>
[RegistroSped(Codigo = "B990", Nivel = 1, Bloco = "B")]
public sealed partial class RegistroB990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B990";

    /// <summary>Quantidade total de linhas do Bloco B, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinB { get; set; }
}

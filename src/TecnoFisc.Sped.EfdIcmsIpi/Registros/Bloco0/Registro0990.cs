using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0990 — Encerramento do Bloco 0. Nível hierárquico 1, ocorrência única por
/// arquivo. O campo QTD_LIN_0 é populado pelo totalizador de blocos (Stage 3) — aqui
/// declara apenas o layout. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 43.
/// </summary>
[RegistroSped(Codigo = "0990", Nivel = 1, Bloco = "0")]
public sealed partial class Registro0990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0990";

    /// <summary>Quantidade total de linhas do Bloco 0, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin0 { get; set; }
}

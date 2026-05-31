using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1990 - Encerramento do Bloco 1. Nivel hierarquico 1, ocorrencia unica por
/// arquivo. O campo QTD_LIN_1 e populado pelo totalizador de blocos (Stage 3) - aqui
/// declara apenas o layout. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 301-302.
/// </summary>
[RegistroSped(Codigo = "1990", Nivel = 1, Bloco = "1")]
public sealed partial class Registro1990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1990";

    /// <summary>Quantidade total de linhas do Bloco 1, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin1 { get; set; }
}

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G990 - Encerramento do Bloco G. Nivel hierarquico 1, ocorrencia unica por
/// arquivo. O campo QTD_LIN_G e populado pelo totalizador de blocos (Stage 3) - aqui
/// declara apenas o layout. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 244.
/// </summary>
[RegistroSped(Codigo = "G990", Nivel = 1, Bloco = "G")]
public sealed partial class RegistroG990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G990";

    /// <summary>Quantidade total de linhas do Bloco G, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinG { get; set; }
}

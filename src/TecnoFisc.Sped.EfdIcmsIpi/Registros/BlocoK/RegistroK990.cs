using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K990 - Encerramento do Bloco K. Nivel hierarquico 1, ocorrencia unica por
/// arquivo. O campo QTD_LIN_K e populado pelo totalizador de blocos (Stage 3) - aqui
/// declara apenas o layout. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 267.
/// </summary>
[RegistroSped(Codigo = "K990", Nivel = 1, Bloco = "K")]
public sealed partial class RegistroK990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K990";

    /// <summary>Quantidade total de linhas do Bloco K, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinK { get; set; }
}

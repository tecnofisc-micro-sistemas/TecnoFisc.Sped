using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

/// <summary>
/// Registro H990 - Encerramento do Bloco H. Nivel hierarquico 1, ocorrencia unica por
/// arquivo. O campo QTD_LIN_H e populado pelo totalizador de blocos (Stage 3) - aqui
/// declara apenas o layout. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 249.
/// </summary>
[RegistroSped(Codigo = "H990", Nivel = 1, Bloco = "H")]
public sealed partial class RegistroH990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "H990";

    /// <summary>Quantidade total de linhas do Bloco H, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinH { get; set; }
}

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J990 — Encerramento do Bloco J. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN_J deve contabilizar todas as linhas do Bloco J, incluindo a abertura (J001) e
/// este registro. A regra REGRA_QTD_LIN_BLOCOJ verifica a consistência do contador.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 208.
/// </summary>
[RegistroSped(Codigo = "J990", Nivel = 1, Bloco = "J")]
public sealed partial class RegistroJ990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J990";

    /// <summary>Quantidade total de linhas do Bloco J, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinJ { get; set; }
}

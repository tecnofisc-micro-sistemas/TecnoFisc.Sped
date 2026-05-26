using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I990 — Encerramento do Bloco I. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN_I deve contabilizar todas as linhas do Bloco I, incluindo a abertura (I001) e
/// este registro. A regra REGRA_QTD_LIN_BLOCOI verifica a consistência do contador.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 169.
/// </summary>
[RegistroSped(Codigo = "I990", Nivel = 1, Bloco = "I")]
public sealed partial class RegistroI990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I990";

    /// <summary>Quantidade total de linhas do Bloco I, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinI { get; set; }
}

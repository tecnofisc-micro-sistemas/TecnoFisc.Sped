using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C990 — Encerramento do Bloco C. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN_0 (nome literal do leiaute ECD) contabiliza todas as linhas do Bloco C,
/// incluindo a abertura (C001) e este registro. Conforme Manual de Orientação do Leiaute 9
/// da ECD, p. 101. A descrição "Bloco 0" na coluna do manual é erro tipográfico — a regra
/// REGRA_QTD_LIN_BLOCO_C confirma que o contador se refere ao Bloco C.
/// </summary>
[RegistroSped(Codigo = "C990", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C990";

    /// <summary>Quantidade total de linhas do Bloco C, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin0 { get; set; }
}

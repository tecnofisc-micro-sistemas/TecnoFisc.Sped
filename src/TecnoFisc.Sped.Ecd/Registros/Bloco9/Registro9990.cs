using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco9;

/// <summary>
/// Registro 9990 — Encerramento do Bloco 9. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN_9 deve contabilizar todas as linhas do Bloco 9, incluindo a abertura (9001),
/// as ocorrências do Registro 9900 e este registro. A regra REGRA_QRD_LIN_BLOCO9 verifica a
/// consistência do contador.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 232.
/// </summary>
[RegistroSped(Codigo = "9990", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9990";

    /// <summary>Quantidade total de linhas do Bloco 9, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin9 { get; set; }
}

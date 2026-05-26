using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco9;

/// <summary>
/// Registro 9999 — Encerramento do Arquivo Digital. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN deve contabilizar todas as linhas do arquivo digital, incluindo este registro.
/// A regra REGRA_QTD_LIN_ARQUIVO verifica a consistência do contador.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 233.
/// </summary>
[RegistroSped(Codigo = "9999", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9999 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9999";

    /// <summary>Quantidade total de linhas do arquivo digital, incluindo este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}

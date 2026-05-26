using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0990 — Encerramento do Bloco 0. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN_0 deve contabilizar todas as linhas do Bloco 0, incluindo abertura (0001) e
/// este registro. Conforme Manual de Orientação do Leiaute 9 da ECD, p. 88.
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

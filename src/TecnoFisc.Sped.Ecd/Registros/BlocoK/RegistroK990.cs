using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K990 — Encerramento do Bloco K. Nível hierárquico 1, ocorrência única por arquivo.
/// O campo QTD_LIN_K deve contabilizar todas as linhas do Bloco K, incluindo a abertura (K001) e
/// este registro. A regra REGRA_QTD_LIN_BLOCOK verifica a consistência do contador.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 229.
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

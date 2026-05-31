using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0035 — Identificação das SCP. Nível hierárquico 2, ocorrência 0:N.
/// Utilizado somente pelas pessoas jurídicas sócias ostensivas que possuem SCP, para
/// identificação das SCP da pessoa jurídica no período da escrituração.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 82.
/// </summary>
/// <remarks>
/// <para>
/// Regra de validação do campo (<c>REGRA_CNPJ_DIFERENTE_SCP</c>): verifica se o
/// <see cref="CodScp"/> é diferente do campo CNPJ (campo 06) do <c>Registro0000</c>.
/// Se a regra não for cumprida o PGE do Sped Contábil gera um erro.
/// Verificação de responsabilidade do consumidor (ARCHITECTURE §2.3).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0035", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0035 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0035";

    /// <summary>
    /// CNPJ da SCP conforme Anexo I, XVIII, da Instrução Normativa RFB nº 2.119/2022.
    /// Campo <c>COD_SCP</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj? CodScp { get; set; }

    /// <summary>Nome da SCP. Campo <c>NOME_SCP</c>.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? NomeScp { get; set; }
}

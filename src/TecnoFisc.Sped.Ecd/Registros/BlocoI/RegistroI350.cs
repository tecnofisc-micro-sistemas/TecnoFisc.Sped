using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I350 — Saldo das Contas de Resultado Antes do Encerramento – Identificação da Data.
/// Nível hierárquico 3, ocorrência 0:N por I010.
/// Identifica a data de apuração do resultado das contas de resultado antes dos lançamentos de
/// encerramento. Os saldos detalhados de cada conta de resultado são fornecidos no Registro I355,
/// filho deste registro.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 155–157.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_DT_RES_DUPLICIDADE</c>: <see cref="DtRes"/> é chave do registro e não pode
///   ser duplicada para o mesmo I010.</item>
///   <item><c>REGRA_ENCERRAMENTO_EXERCICIO</c>: quando <c>IND_ESC</c> (I010) for "G", "R" ou "B",
///   deve existir pelo menos um I350 com <see cref="DtRes"/> igual a <c>DT_EX_SOCIAL</c>
///   (campo 12 do Registro I030).</item>
///   <item><c>REGRA_DATA_INTERVALO_DO_ARQUIVO</c>: <see cref="DtRes"/> deve estar dentro do
///   intervalo [DT_INI, DT_FIN] do Registro 0000.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I350", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI350 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I350";

    /// <summary>
    /// Data da apuração do resultado (ddMMyyyy). Chave do registro (<c>REGRA_DT_RES_DUPLICIDADE</c>).
    /// Campo <c>DT_RES</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRes { get; set; }
}

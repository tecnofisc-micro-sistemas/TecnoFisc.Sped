using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I300 — Balancetes Diários – Identificação da Data. Nível hierárquico 3, ocorrência 0:N por I010.
/// Identifica a data do balancete diário. Utilizado apenas quando o tipo de escrituração é "B"
/// (Livro de Balancetes Diários e Balanços), conforme <c>IND_ESC</c> do Registro I010.
/// Os detalhes do balancete (totais de débitos e créditos por conta/centro de custos) são
/// fornecidos no Registro I310, filho deste registro.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 152–153.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_DATA_BALANCETE_DUPLICADO</c>: <see cref="DtBcte"/> é chave do registro e
///   não pode ser duplicada para o mesmo I010.</item>
///   <item><c>REGRA_DATA_INTERVALO_DO_ARQUIVO</c>: <see cref="DtBcte"/> deve estar dentro do
///   intervalo [DT_INI, DT_FIN] do Registro 0000.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I300", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I300";

    /// <summary>
    /// Data do balancete (ddMMyyyy). Chave do registro (<c>REGRA_DATA_BALANCETE_DUPLICADO</c>).
    /// Campo <c>DT_BCTE</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtBcte { get; set; }
}

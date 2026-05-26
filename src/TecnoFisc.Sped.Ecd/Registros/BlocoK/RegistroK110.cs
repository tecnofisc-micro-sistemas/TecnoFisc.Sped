using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K110 — Relação dos Eventos Societários. Nível hierárquico 4, ocorrência 0:N
/// por arquivo. Demonstra a relação dos eventos societários das empresas consolidadas.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 216–217.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_OBRIGATORIO_K115</c>: deve existir pelo menos um K115 filho quando
///   <see cref="Evento"/> for <see cref="EventoSocietario.Aquisicao"/>, <see cref="EventoSocietario.Alienacao"/>,
///   <see cref="EventoSocietario.Fusao"/>, <see cref="EventoSocietario.CisaoParcial"/>,
///   <see cref="EventoSocietario.CisaoTotal"/> ou <see cref="EventoSocietario.Incorporacao"/>.</item>
///   <item><c>REGRA_REGISTRO_NAO_DEVE_EXISTIR_K115</c>: não deve existir K115 filho quando
///   <see cref="Evento"/> for <see cref="EventoSocietario.Extincao"/> ou
///   <see cref="EventoSocietario.Constituicao"/>.</item>
///   <item><c>REGRA_SOMATORIO_PER_EVT_K115</c>: o somatório dos percentuais das empresas
///   participantes (PER_EVT do K115) deve ser menor ou igual a 100.</item>
///   <item><c>REGRA_ANO_IGUAL_ANTERIOR_POSTERIOR_K030</c>: o ano de <see cref="DtEvento"/>
///   deve ser igual ao ano da data final do período consolidado (DT_FIN do K030), ou ao ano
///   anterior, ou ao ano posterior.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K110", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K110";

    /// <summary>
    /// Evento societário ocorrido no período.
    /// Campo <c>EVENTO</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public EventoSocietario Evento { get; set; }

    /// <summary>
    /// Data do evento societário (ddMMyyyy).
    /// Campo <c>DT_EVENTO</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtEvento { get; set; }
}

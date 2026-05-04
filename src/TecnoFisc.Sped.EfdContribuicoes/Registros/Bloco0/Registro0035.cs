using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0035 — Identificação de Sociedade em Conta de Participação (SCP). Nível
/// hierárquico 2, ocorrência 1:N. Conforme Guia Prático v1.35, p. 69-70. Obrigatório
/// quando o campo IND_NAT_PJ do <see cref="Registro0000"/> for 03, 04 ou 05; nesses
/// casos identifica cada SCP em que a pessoa jurídica titular participa como sócia
/// ostensiva (ou a própria SCP, quando IND_NAT_PJ = 05).
/// </summary>
[RegistroSped(Codigo = "0035", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0035 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0035";

    /// <summary>
    /// Identificação da SCP — código numérico de tamanho fixo 14, livre definição pela
    /// PJ sócia ostensiva. Para fatos geradores a partir de abr/2021 deve ser o CNPJ
    /// da SCP (com DV válido e base distinta da informada no Registro 0000).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14)]
    public string? CodScp { get; set; }

    /// <summary>Descrição da SCP — objeto do empreendimento ou atividade para a qual foi constituída.</summary>
    [CampoSped(Ordem = 3)]
    public string? DescScp { get; set; }

    /// <summary>Informação complementar da SCP.</summary>
    [CampoSped(Ordem = 4)]
    public string? InfComp { get; set; }
}

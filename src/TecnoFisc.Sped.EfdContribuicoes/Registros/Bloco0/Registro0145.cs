using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0145 — Regime de Apuração da Contribuição Previdenciária Sobre a Receita Bruta.
/// Nível hierárquico 3, ocorrência 1:1 (por estabelecimento Registro0140).
/// Conforme Guia Prático v1.35, p. 79-81.
/// </summary>
[RegistroSped(Codigo = "0145", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0145 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0145";

    /// <summary>Código indicador da incidência da CPRB no período (1=somente receita; 2=receita e remunerações).</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public CodigoIncidenciaContribPrevidenciaria CodIncTrib { get; set; }

    /// <summary>Valor da Receita Bruta Total da Pessoa Jurídica no Período (consolidada da empresa).</summary>
    [CampoSped(Ordem = 3, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecTot { get; set; }

    /// <summary>Valor da Receita Bruta da(s) Atividade(s) Sujeita(s) à CPRB.</summary>
    [CampoSped(Ordem = 4, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecAtiv { get; set; }

    /// <summary>Valor da Receita Bruta da(s) Atividade(s) não Sujeita(s) à CPRB.</summary>
    [CampoSped(Ordem = 5, Decimais = 2)]
    public decimal? VlRecDemaisAtiv { get; set; }

    /// <summary>Informação complementar.</summary>
    [CampoSped(Ordem = 6)]
    public string? InfoCompl { get; set; }
}

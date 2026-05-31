using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1020 — Processo Referenciado – Processo Administrativo.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Referencia decisões administrativas
/// da Receita Federal do Brasil que autorizam procedimento específico de apuração das
/// contribuições sociais ou dos créditos. Conforme Guia Prático EFD Contribuições v1.35, p. 381.
/// </summary>
[RegistroSped(Codigo = "1020", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1020";

    /// <summary>Identificação do Processo Administrativo ou da Decisão Administrativa (até 20 caracteres).</summary>
    [CampoSped(Ordem = 2, Tamanho = 20, Obrigatorio = true)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da natureza da ação decorrente de Processo Administrativo na Secretaria da Receita Federal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorNaturezaAcaoAdministrativa IndNatAcao { get; set; }

    /// <summary>Data do Despacho/Decisão Administrativa no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDecAdm { get; set; }
}

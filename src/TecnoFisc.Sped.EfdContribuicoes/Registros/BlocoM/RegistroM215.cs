using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M215 — Ajustes da Base de Cálculo da Contribuição para o PIS/Pasep Apurada.
/// Nível hierárquico 4, ocorrência 1:N. Filho de M210.
/// Habilitado apenas para fatos geradores a partir de 01/01/2019.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 319.
/// </summary>
[RegistroSped(Codigo = "M215", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM215 : RegistroSped
{
    public override string Codigo => "M215";

    /// <summary>Indicador do tipo de ajuste da base de cálculo: 0=Redução; 1=Acréscimo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorAjuste IndAjBc { get; set; }

    /// <summary>Valor do ajuste de base de cálculo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjBc { get; set; }

    /// <summary>Código do ajuste, conforme Tabela 4.3.18.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? CodAjBc { get; set; }

    /// <summary>Número do processo, documento ou ato concessório ao qual o ajuste está vinculado.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? NumDoc { get; set; }

    /// <summary>Descrição resumida do ajuste na base de cálculo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? DescrAjBc { get; set; }

    /// <summary>Data de referência do ajuste (ddmmaaaa).</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRef { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 8, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>CNPJ do estabelecimento a que se refere o ajuste.</summary>
    [CampoSped(Ordem = 9, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Informação complementar do registro.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}

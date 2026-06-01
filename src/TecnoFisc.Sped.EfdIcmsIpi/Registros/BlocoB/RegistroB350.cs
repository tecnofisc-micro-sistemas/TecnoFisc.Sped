using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B350 — Serviços Prestados por Instituições Financeiras.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 50-51.
/// </summary>
[RegistroSped(Codigo = "B350", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB350 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B350";

    /// <summary>Código da conta do plano de contas do declarante.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtd { get; set; }

    /// <summary>Descrição da conta de receita no plano de contas do declarante.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CtaIss { get; set; }

    /// <summary>Código COSIF (8 dígitos) subordinado à conta do ISS das instituições financeiras, conforme Tabela 4.6.2.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Obrigatorio = true)]
    public string? CtaCosif { get; set; }

    /// <summary>Quantidade de Registros B350 que referenciaram o mesmo COD_CTD no período. Mínimo 1.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int? QtdOcor { get; set; }

    /// <summary>Item da lista de serviços (LC 116/2003), conforme Tabela 4.6.3. Tamanho fixo 4 dígitos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 4, Obrigatorio = true)]
    public string? CodServ { get; set; }

    /// <summary>Valor das prestações referentes à conta de receita.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCont { get; set; }

    /// <summary>Valor da base de cálculo do ISS correspondente às prestações da conta de receita.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIss { get; set; }

    /// <summary>Alíquota de incidência do ISS. Máximo 5%.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal AliqIss { get; set; }

    /// <summary>Valor do ISS sobre as prestações da conta de receita.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIss { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460). Máx. 60 caracteres.</summary>
    [CampoSped(Ordem = 11, Tamanho = 60)]
    public string? CodInfObs { get; set; }
}

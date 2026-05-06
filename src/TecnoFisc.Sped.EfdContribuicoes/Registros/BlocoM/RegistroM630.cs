using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M630 — Informações Adicionais de Diferimento.
/// Nível hierárquico 4, ocorrência 1:N (filho de M610).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 357.
/// </summary>
[RegistroSped(Codigo = "M630", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM630 : RegistroSped
{
    public override string Codigo => "M630";

    /// <summary>CNPJ da pessoa jurídica de direito público, empresa pública ou sociedade de economia mista.</summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj CnpjPj { get; set; }

    /// <summary>Valor Total das vendas no período.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlVend { get; set; }

    /// <summary>Valor Total não recebido no período.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlNaoReceb { get; set; }

    /// <summary>Valor da Contribuição diferida no período.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContDif { get; set; }

    /// <summary>Valor do Crédito diferido no período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredDif { get; set; }

    /// <summary>Código de Tipo de Crédito diferido no período, conforme Tabela 4.3.6.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3)]
    public CodigoTipoCredito? CodCred { get; set; }
}

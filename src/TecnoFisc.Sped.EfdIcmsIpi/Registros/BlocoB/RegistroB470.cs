using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B470 — Apuração do ISS.
/// Nível hierárquico 2, ocorrência um (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 55.
/// </summary>
[RegistroSped(Codigo = "B470", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB470 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B470";

    /// <summary>Valor total referente às prestações de serviço do período.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCont { get; set; }

    /// <summary>Valor total do material fornecido por terceiros na prestação do serviço.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlMatTerc { get; set; }

    /// <summary>Valor do material próprio utilizado na prestação do serviço.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlMatProp { get; set; }

    /// <summary>Valor total das subempreitadas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSub { get; set; }

    /// <summary>Valor total das operações isentas ou não-tributadas pelo ISS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIsnt { get; set; }

    /// <summary>Valor total das deduções da base de cálculo (B + C + D + E).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDedBc { get; set; }

    /// <summary>Valor total da base de cálculo do ISS.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIss { get; set; }

    /// <summary>Valor total da base de cálculo do ISS referente às prestações do declarante.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIssRt { get; set; }

    /// <summary>Valor total do ISS destacado.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIss { get; set; }

    /// <summary>Valor total do ISS retido pelo tomador nas prestações do declarante.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssRt { get; set; }

    /// <summary>Valor total das deduções do ISS próprio.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDed { get; set; }

    /// <summary>Valor total apurado do ISS próprio a recolher (I - J - K).</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssRec { get; set; }

    /// <summary>Valor total do ISS substituto a recolher pelas aquisições do declarante (tomador).</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssSt { get; set; }

    /// <summary>Valor do ISS próprio a recolher pela Sociedade Uniprofissional.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssRecUni { get; set; }
}

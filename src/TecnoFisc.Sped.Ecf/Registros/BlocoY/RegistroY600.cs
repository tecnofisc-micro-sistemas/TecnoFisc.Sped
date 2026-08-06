using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y600 - identificação e remuneração de sócios e dirigentes.</summary>
[RegistroSped(Codigo = "Y600", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y600";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAltSoc { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFimSoc { get; set; }

    /// <summary>Código de país preservado lexicalmente.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public TipoQualificacaoSocio IndQualif { get; set; }

    /// <summary>CPF ou CNPJ preservado como documento composto condicional.</summary>
    [CampoSped(Ordem = 6, Tamanho = 14)]
    public string? CpfCnpj { get; set; }

    [CampoSped(Ordem = 7, Obrigatorio = true)]
    public string? NomEmp { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 2, Obrigatorio = true)]
    public QualificacaoSocio Qualif { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PercCapTot { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PercCapVot { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 11)]
    public Cpf? CpfRepLeg { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 2)]
    public QualificacaoRepresentanteLegal? QualifRepLeg { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlRemTrab { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlLucDiv { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlJurCap { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlDemRend { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlIrRet { get; set; }
}

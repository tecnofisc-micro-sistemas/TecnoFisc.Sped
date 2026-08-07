using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y570 - imposto de renda e CSLL retidos na fonte.</summary>
[RegistroSped(Codigo = "Y570", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY570 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y570";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true, Nome = "CNPJ_FON")]
    public Cnpj CnpjFon { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "NOM_EMP")]
    public string? NomEmp { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "IND_ORG_PUB")]
    public IndicadorSimNao IndOrgPub { get; set; }

    /// <summary>Código de receita preservado lexicalmente.</summary>
    [CampoSped(Ordem = 5, Tamanho = 4, Obrigatorio = true, Nome = "COD_REC")]
    public string? CodRec { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_REND")]
    public decimal VlRend { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Nome = "IR_RET")]
    public decimal? IrRet { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Nome = "CSLL_RET")]
    public decimal? CsllRet { get; set; }
}

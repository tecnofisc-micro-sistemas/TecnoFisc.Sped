using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y612 - rendimentos de dirigentes e conselheiros de imunes ou isentas.</summary>
[RegistroSped(Codigo = "Y612", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY612 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y612";

    [CampoSped(Ordem = 2, Tamanho = 11, Obrigatorio = true, Nome = "CPF")]
    public Cpf Cpf { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "NOME")]
    public string? Nome { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true, Nome = "QUALIF")]
    public QualificacaoDirigenteConselheiro Qualif { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_REM_TRAB")]
    public decimal VlRemTrab { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_DEM_REND")]
    public decimal VlDemRend { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_IR_RET")]
    public decimal VlIrRet { get; set; }
}

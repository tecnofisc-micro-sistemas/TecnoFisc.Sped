using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W250 - entidade integrante da Declaração País-a-País.</summary>
[RegistroSped(Codigo = "W250", Nivel = 4, Bloco = "W")]
public sealed partial class RegistroW250 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W250";

    [CampoSped(Ordem = 2, Tamanho = 2)]
    public string? JurDiferente { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>TIN genérico, preservado sem inferir CNPJ.</summary>
    [CampoSped(Ordem = 4, Obrigatorio = true)]
    public string? Tin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? JurisdicaoTin { get; set; }

    /// <summary>Identificador substituto genérico.</summary>
    [CampoSped(Ordem = 6)]
    public string? Ni { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 2)]
    public string? JurisdicaoNi { get; set; }

    [CampoSped(Ordem = 8)]
    public string? TipoNi { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 7, Obrigatorio = true)]
    public TipoEnderecoDpp TipEnd { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 150, Obrigatorio = true)]
    public string? Endereço { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 15)]
    public string? NumTel { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 115)]
    public string? Email { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ1 { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ2 { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ3 { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ4 { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ5 { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ6 { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ7 { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ8 { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ9 { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ10 { get; set; }

    [CampoSped(Ordem = 23, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ11 { get; set; }

    [CampoSped(Ordem = 24, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ12 { get; set; }

    [CampoSped(Ordem = 25, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Ativ13 { get; set; }

    [CampoSped(Ordem = 26, Tamanho = 255)]
    public string? DescOutros { get; set; }

    [CampoSped(Ordem = 27, Tamanho = 1000)]
    public string? Observação { get; set; }
}

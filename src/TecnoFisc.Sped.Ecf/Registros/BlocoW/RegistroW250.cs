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

    [CampoSped(Ordem = 2, Tamanho = 2, Nome = "JUR_DIFERENTE")]
    public string? JurDiferente { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "NOME")]
    public string? Nome { get; set; }

    /// <summary>TIN genérico, preservado sem inferir CNPJ.</summary>
    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "TIN")]
    public string? Tin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true, Nome = "JURISDICAO_TIN")]
    public string? JurisdicaoTin { get; set; }

    /// <summary>Identificador substituto genérico.</summary>
    [CampoSped(Ordem = 6, Nome = "NI")]
    public string? Ni { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 2, Nome = "JURISDICAO_NI")]
    public string? JurisdicaoNi { get; set; }

    [CampoSped(Ordem = 8, Nome = "TIPO_NI")]
    public string? TipoNi { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 7, Obrigatorio = true, Nome = "TIP_END")]
    public TipoEnderecoDpp TipEnd { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 150, Obrigatorio = true, Nome = "ENDEREÇO")]
    public string? Endereço { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 15, Nome = "NUM_TEL")]
    public string? NumTel { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 115, Nome = "EMAIL")]
    public string? Email { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_1")]
    public IndicadorSimNao Ativ1 { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_2")]
    public IndicadorSimNao Ativ2 { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_3")]
    public IndicadorSimNao Ativ3 { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_4")]
    public IndicadorSimNao Ativ4 { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_5")]
    public IndicadorSimNao Ativ5 { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_6")]
    public IndicadorSimNao Ativ6 { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_7")]
    public IndicadorSimNao Ativ7 { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_8")]
    public IndicadorSimNao Ativ8 { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_9")]
    public IndicadorSimNao Ativ9 { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_10")]
    public IndicadorSimNao Ativ10 { get; set; }

    [CampoSped(Ordem = 23, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_11")]
    public IndicadorSimNao Ativ11 { get; set; }

    [CampoSped(Ordem = 24, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_12")]
    public IndicadorSimNao Ativ12 { get; set; }

    [CampoSped(Ordem = 25, Tamanho = 1, Obrigatorio = true, Nome = "ATIV_13")]
    public IndicadorSimNao Ativ13 { get; set; }

    [CampoSped(Ordem = 26, Tamanho = 255, Nome = "DESC_OUTROS")]
    public string? DescOutros { get; set; }

    [CampoSped(Ordem = 27, Tamanho = 1000, Nome = "OBSERVAÇÃO")]
    public string? Observação { get; set; }
}

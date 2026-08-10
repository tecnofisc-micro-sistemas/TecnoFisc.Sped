using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X485 - benefícios fiscais, parte II.</summary>
[RegistroSped(Codigo = "X485", Nivel = 2, Bloco = "X", IntroduzidoEm = (int)LayoutEcf.V010)]
public sealed partial class RegistroX485 : RegistroSped
{
    public override string Codigo => "X485";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true, Nome = "TIPO_BENEF")]
    public TipoBeneficioFiscal TipoBenef { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "ATO_DECL")]
    public string? AtoDecl { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 14, Nome = "CNPJ_INCORP")]
    public Cnpj? CnpjIncorp { get; set; }

    /// <summary>Identificador numérico preservado lexicalmente por admitir zeros à esquerda.</summary>
    [CampoSped(Ordem = 5, Tamanho = 18, Nome = "ID_OBRA_2018")]
    public string? IdObra2018 { get; set; }

    /// <summary>Identificador numérico preservado lexicalmente por admitir zeros à esquerda.</summary>
    [CampoSped(Ordem = 6, Tamanho = 18, Nome = "ID_OBRA_2020")]
    public string? IdObra2020 { get; set; }

    /// <summary>Identificador numérico preservado lexicalmente por admitir zeros à esquerda.</summary>
    [CampoSped(Ordem = 7, Tamanho = 18, Nome = "ID_OBRA_EEI")]
    public string? IdObraEei { get; set; }

    /// <summary>Portaria no formato lexical NNN/AAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 7, Nome = "PORT_CEBAS")]
    public string? PortCebas { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_DOU_PORT_CEBAS")]
    public DateOnly? DtDouPortCebas { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_INI_PORT_CEBAS")]
    public DateOnly? DtIniPortCebas { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_FIN_PORT_CEBAS")]
    public DateOnly? DtFinPortCebas { get; set; }
}

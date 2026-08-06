using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X485 - benefícios fiscais, parte II.</summary>
[RegistroSped(Codigo = "X485", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX485 : RegistroSped
{
    public override string Codigo => "X485";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public TipoBeneficioFiscal TipoBenef { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public string? AtoDecl { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 14)]
    public Cnpj? CnpjIncorp { get; set; }

    /// <summary>Identificador numérico preservado lexicalmente por admitir zeros à esquerda.</summary>
    [CampoSped(Ordem = 5, Tamanho = 18)]
    public string? IdObra2018 { get; set; }

    /// <summary>Identificador numérico preservado lexicalmente por admitir zeros à esquerda.</summary>
    [CampoSped(Ordem = 6, Tamanho = 18)]
    public string? IdObra2020 { get; set; }

    /// <summary>Identificador numérico preservado lexicalmente por admitir zeros à esquerda.</summary>
    [CampoSped(Ordem = 7, Tamanho = 18)]
    public string? IdObraEei { get; set; }

    /// <summary>Portaria no formato lexical NNN/AAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 7)]
    public string? PortCebas { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDouPortCebas { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIniPortCebas { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinPortCebas { get; set; }
}

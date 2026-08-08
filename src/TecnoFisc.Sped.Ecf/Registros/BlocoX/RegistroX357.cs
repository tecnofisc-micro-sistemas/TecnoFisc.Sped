using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X357 - investidoras diretas.</summary>
[RegistroSped(Codigo = "X357", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX357 : RegistroSped
{
    public override string Codigo => "X357";

    /// <summary>País conforme tabela dinâmica do Sped, preservado lexicalmente.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true, Nome = "PAIS")]
    public string? Pais { get; set; }

    /// <summary>
    /// NIF ou CNPJ composto, sem coerção para um identificador brasileiro.
    /// Nome normativo do manual (pág. 474) é <c>NIF/CNPJ</c> — nome composto legítimo do
    /// RFB (o campo aceita NIF ou CNPJ conforme o país da investidora), não erro de
    /// extração do PDF. O alias abaixo normaliza a barra para underscore porque não é um
    /// separador válido em <c>Nome</c> (CatalogoBuilder.IsFieldNameValid).
    /// </summary>
    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "NIF_CNPJ")]
    public string? NifCnpj { get; set; }

    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "RAZAO_SOCIAL")]
    public string? RazaoSocial { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Decimais = 4, Obrigatorio = true, Nome = "PERCENTUAL")]
    public decimal Percentual { get; set; }
}

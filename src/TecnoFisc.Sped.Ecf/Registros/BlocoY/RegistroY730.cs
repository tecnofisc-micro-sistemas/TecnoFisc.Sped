using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y730 - identificação de donatários e destinatários de deduções.</summary>
[RegistroSped(Codigo = "Y730", Nivel = 2, Bloco = "Y", IntroduzidoEm = (int)LayoutEcf.V012)]
public sealed partial class RegistroY730 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y730";

    /// <summary>Código dinâmico da dedução, preservado lexicalmente.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true, Nome = "DEDUCAO")]
    public string? Deducao { get; set; }

    /// <summary>Código dinâmico do tipo de doação, preservado lexicalmente.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true, Nome = "TIPO")]
    public string? Tipo { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DATA")]
    public DateOnly Data { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true, Nome = "TIPO_DESTINATARIO")]
    public TipoDestinatarioDeducao TipoDestinatario { get; set; }

    /// <summary>CPF ou CNPJ preservado como documento composto condicional.</summary>
    [CampoSped(Ordem = 6, Tamanho = 14, Obrigatorio = true, Nome = "DESTINATARIO")]
    public string? Destinatario { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VALOR")]
    public decimal Valor { get; set; }

    [CampoSped(Ordem = 8, Nome = "OBSERVACAO")]
    public string? Observacao { get; set; }
}

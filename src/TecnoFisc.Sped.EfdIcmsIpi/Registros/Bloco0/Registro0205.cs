using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0205 — Alteração do Item. Nível hierárquico 3, ocorrência 1:N por Registro0200.
/// Informa alterações na descrição ou codificação do item sem descaracterizá-lo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 35-36.
/// </summary>
[RegistroSped(Codigo = "0205", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0205 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0205";

    /// <summary>Descrição anterior do item. Mutuamente excludente com CodAntItem; obrigatório preencher um dos dois.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? DescrAntItem { get; set; }

    /// <summary>Data inicial de utilização da descrição anterior (ddMMyyyy). Deve ser data válida.</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final de utilização da descrição anterior (ddMMyyyy). Deve ser data válida e menor que DT_FIN do Registro0000.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFim { get; set; }

    /// <summary>Código anterior do item (até 60 caracteres). Mutuamente excludente com DescrAntItem; obrigatório preencher um dos dois.</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodAntItem { get; set; }
}

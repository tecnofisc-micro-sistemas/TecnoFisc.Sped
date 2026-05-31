using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E115 — Informações Adicionais da Apuração do ICMS — Valores Declaratórios.
/// Nível hierárquico 4, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 212.
/// </summary>
[RegistroSped(Codigo = "E115", Nivel = 4, Bloco = "E")]
public sealed partial class RegistroE115 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E115";

    /// <summary>Código da informação adicional, conforme tabela definida pelas SEFAZ (item 5.2 do guia).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true)]
    public string? CodInfAdic { get; set; }

    /// <summary>Valor referente à informação adicional.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlInfAdic { get; set; }

    /// <summary>Descrição complementar do ajuste.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescrComplAj { get; set; }
}

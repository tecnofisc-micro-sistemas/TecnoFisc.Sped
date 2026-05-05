using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C395 — NF de Venda a Consumidor (Códigos 02, 2D, 2E, 59, 60 e 65) – Aquisições/Entradas com Crédito.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 151.
/// </summary>
[RegistroSped(Codigo = "C395", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC395 : RegistroSped
{
    public override string Codigo => "C395";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodPart { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 3)]
    public string? SubSer { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public string? NumDoc { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }
}

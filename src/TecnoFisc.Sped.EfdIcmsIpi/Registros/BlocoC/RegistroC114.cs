using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C114 — Cupom Fiscal Referenciado.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 70.
/// </summary>
[RegistroSped(Codigo = "C114", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC114 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C114";

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (valores válidos: 02, 2D, 2E).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Número de série de fabricação do ECF.</summary>
    [CampoSped(Ordem = 3, Tamanho = 21, Obrigatorio = true)]
    public string? EcfFab { get; set; }

    /// <summary>Número da caixa atribuído ao ECF.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public int? EcfCx { get; set; }

    /// <summary>Número do documento fiscal (Contador de Ordem de Operação — COO).</summary>
    [CampoSped(Ordem = 5, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do cupom no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }
}

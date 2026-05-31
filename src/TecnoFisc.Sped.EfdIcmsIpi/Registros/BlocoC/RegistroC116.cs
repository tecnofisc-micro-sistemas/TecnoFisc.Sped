using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C116 — Cupom Fiscal Eletrônico — CF-e Referenciado.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 71.
/// </summary>
[RegistroSped(Codigo = "C116", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC116 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C116";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1 (valor válido: 59 — CF-e-SAT).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Número de série do equipamento SAT (9 dígitos).</summary>
    [CampoSped(Ordem = 3, Tamanho = 9, Obrigatorio = true)]
    public int? NrSat { get; set; }

    /// <summary>Chave do Cupom Fiscal Eletrônico CF-e-SAT (44 dígitos).</summary>
    [CampoSped(Ordem = 4, Tamanho = 44, Obrigatorio = true)]
    public string? ChvCfe { get; set; }

    /// <summary>Número do cupom fiscal eletrônico (6 dígitos).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public int? NumCfe { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }
}

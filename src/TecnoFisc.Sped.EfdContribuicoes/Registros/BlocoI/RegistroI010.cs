using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I010 — Identificação da Pessoa Jurídica/Estabelecimento.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 285.
/// </summary>
[RegistroSped(Codigo = "I010", Nivel = 2, Bloco = "I")]
public sealed partial class RegistroI010 : RegistroSped
{
    public override string Codigo => "I010";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorAtividadeInstituicaoFinanceira IndAtiv { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}

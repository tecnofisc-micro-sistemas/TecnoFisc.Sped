using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P010 — Identificação do Estabelecimento. Nível hierárquico 2, ocorrência vários
/// (por arquivo). Deve ser escriturado apenas para estabelecimentos que efetivamente tenham
/// auferido receitas sujeitas à incidência da CPRB no período. Conforme Guia Prático v1.35, p. 367.
/// </summary>
[RegistroSped(Codigo = "P010", Nivel = 2, Bloco = "P")]
public sealed partial class RegistroP010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P010";

    /// <summary>
    /// CNPJ do estabelecimento (14 dígitos, sem máscara). Validação: DV conferido. O estabelecimento
    /// deve estar cadastrado no Registro 0140 com o registro filho 0145 preenchido.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }
}

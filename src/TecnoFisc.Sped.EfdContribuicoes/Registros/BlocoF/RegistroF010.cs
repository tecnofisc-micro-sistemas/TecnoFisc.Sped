using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F010 — Identificação do Estabelecimento. Nível hierárquico 2, ocorrência vários
/// por arquivo. Escriturado apenas para estabelecimentos que efetivamente realizaram operações
/// passíveis de escrituração no Bloco F. Conforme Guia Prático v1.35, p. 230.
/// </summary>
[RegistroSped(Codigo = "F010", Nivel = 2, Bloco = "F")]
public sealed partial class RegistroF010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "F010";

    /// <summary>
    /// CNPJ do estabelecimento (14 dígitos, sem máscara). Validação: DV conferido. O estabelecimento
    /// deve estar cadastrado no Registro 0140.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }
}

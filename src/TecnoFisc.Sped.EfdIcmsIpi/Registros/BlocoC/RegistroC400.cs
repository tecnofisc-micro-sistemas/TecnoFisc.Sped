using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C400 — Identificação do Equipamento ECF (código 02, 2D e 60).
/// Deve ser informado por todos os contribuintes que utilizem ECF na emissão de documentos fiscais.
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 119.
/// </summary>
[RegistroSped(Codigo = "C400", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC400 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C400";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1 (valores válidos: 02, 2D, 60).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Modelo do equipamento ECF.</summary>
    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true)]
    public string? EcfMod { get; set; }

    /// <summary>Número de série de fabricação do ECF.</summary>
    [CampoSped(Ordem = 4, Tamanho = 21, Obrigatorio = true)]
    public string? EcfFab { get; set; }

    /// <summary>Número do caixa atribuído ao ECF pelo estabelecimento.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3, Obrigatorio = true)]
    public int? EcfCx { get; set; }
}

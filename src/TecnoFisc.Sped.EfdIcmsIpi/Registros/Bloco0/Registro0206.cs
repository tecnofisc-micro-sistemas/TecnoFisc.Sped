using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0206 — Código de Produto Conforme Tabela Publicada pela ANP. Nível hierárquico 3,
/// ocorrência 1:1 por Registro0200. Informado apenas por produtores, importadores, distribuidores
/// e postos de combustíveis. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 36-37.
/// </summary>
[RegistroSped(Codigo = "0206", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0206 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0206";

    /// <summary>Código do produto conforme Tabela de Produtos para Combustíveis/Solventes (SIMP) da ANP — Tabela 12 do Ato COTEPE/ICMS nº 44/2018.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodComb { get; set; }
}

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C601 — Documentos Cancelados da Consolidação Diária de NF/Contas de Energia Elétrica
/// (cód. 06), NF/Conta de Fornecimento D'Água Canalizada (cód. 29) e NF/Conta de Fornecimento de
/// Gás Canalizado (cód. 28). Filho do C600; informa a numeração de cada documento cancelado.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 145.
/// </summary>
[RegistroSped(Codigo = "C601", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC601 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C601";

    /// <summary>Número do documento fiscal cancelado. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 2, Tamanho = 9, Obrigatorio = true)]
    public int? NumDocCanc { get; set; }
}

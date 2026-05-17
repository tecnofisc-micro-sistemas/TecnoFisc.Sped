namespace TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

/// <summary>
/// Versões do leiaute EFD ICMS-IPI cobertas pela janela fiscal de 5 anos. O número da
/// constante é o <c>COD_VER</c> do registro <c>0000</c> (Tabela "Versão do Leiaute" do
/// Manual de Orientação, item 3.1.1 da Nota Técnica conforme Ato COTEPE/ICMS nº 44/2018
/// e alterações). Atualmente só a versão vigente é modelada — incrementos posteriores
/// entram à medida que a Receita publica novas Notas Técnicas.
/// </summary>
/// <remarks>
/// <para>
/// <b>Versão do leiaute ≠ versão do Guia Prático.</b> "Versão 3.2.2" identifica o
/// documento Guia Prático EFD-ICMS/IPI publicado pelo Confaz; o leiaute do arquivo
/// fiscal evolui em uma numeração própria (015, 016, …) mantida na Nota Técnica.
/// Múltiplas versões do Guia podem descrever o mesmo leiaute. Este enum modela o
/// leiaute, não o guia.
/// </para>
/// <para>
/// <b>Convenção numérica.</b> O valor inteiro de cada constante é exatamente o
/// <c>COD_VER</c> esperado no registro <c>0000</c>: <c>V015 = 15</c>. Permite usar o cast
/// direto como argumento de atributo
/// (<c>[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V015)]</c>) e comparar versões
/// aritmeticamente.
/// </para>
/// <para>
/// <b>Janela fiscal de 5 anos.</b> Versões anteriores a V015 (vigentes antes de 2021-01)
/// ficaram fora da janela fiscal e não são modeladas — dados desse período não podem
/// mais ser escriturados via SPED. Quando a Receita avançar o corte e V015 sair da
/// janela, a constante mais antiga é removida em PR dedicado.
/// </para>
/// </remarks>
public enum LayoutEfdIcmsIpi
{
    /// <summary>Leiaute 015 — vigente a partir do período de apuração de janeiro/2021 (NT 2020.001, Guia Prático 3.0.6 em diante).</summary>
    V015 = 15,
}

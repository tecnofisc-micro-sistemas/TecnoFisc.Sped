namespace TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

/// <summary>
/// Versões do leiaute EFD ICMS-IPI cobertas pela janela fiscal de 5 anos. A primeira versão
/// implementada (V306) é o baseline; as 16 subsequentes (V307 → V322) são incrementos sobre
/// ela, modelados via <c>CampoSpedAttribute.DesdeVersao</c> e
/// <c>RegistroSpedAttribute.IntroduzidoEm</c>. Total: 17 versões.
/// </summary>
/// <remarks>
/// <para>
/// <b>Convenção numérica.</b> O valor inteiro de cada constante codifica a versão SemVer-like
/// da Receita Federal sem o ponto: <c>3.0.6</c> → <c>306</c>, <c>3.2.2</c> → <c>322</c>. Permite
/// usar o cast direto como argumento de atributo:
/// <code>
/// [CampoSped(Ordem = 27, DesdeVersao = (int)LayoutEfdIcmsIpi.V310)]
/// </code>
/// e comparar versões aritmeticamente (<c>versaoArquivo &gt;= (int)V310</c>).
/// </para>
/// <para>
/// <b>Janela fiscal de 5 anos.</b> Versões anteriores a V306 (vigentes antes de 2021-01) não
/// são modeladas — dados desse período não podem mais ser escriturados via SPED. Quando a
/// Receita publicar V323 e o corte avançar para além de V306, a constante mais antiga sai do
/// enum em PR dedicado.
/// </para>
/// </remarks>
public enum LayoutEfdIcmsIpi
{
    /// <summary>Versão 3.0.6 — baseline implementada em Stage 8.</summary>
    V306 = 306,

    /// <summary>Versão 3.0.7 — incremento sobre V306.</summary>
    V307 = 307,

    /// <summary>Versão 3.0.8 — incremento sobre V307.</summary>
    V308 = 308,

    /// <summary>Versão 3.0.9 — incremento sobre V308.</summary>
    V309 = 309,

    /// <summary>Versão 3.1.0 — incremento sobre V309.</summary>
    V310 = 310,

    /// <summary>Versão 3.1.1 — incremento sobre V310.</summary>
    V311 = 311,

    /// <summary>Versão 3.1.2 — incremento sobre V311.</summary>
    V312 = 312,

    /// <summary>Versão 3.1.3 — incremento sobre V312.</summary>
    V313 = 313,

    /// <summary>Versão 3.1.4 — incremento sobre V313.</summary>
    V314 = 314,

    /// <summary>Versão 3.1.5 — incremento sobre V314.</summary>
    V315 = 315,

    /// <summary>Versão 3.1.6 — incremento sobre V315.</summary>
    V316 = 316,

    /// <summary>Versão 3.1.7 — incremento sobre V316.</summary>
    V317 = 317,

    /// <summary>Versão 3.1.8 — incremento sobre V317.</summary>
    V318 = 318,

    /// <summary>Versão 3.1.9 — incremento sobre V318.</summary>
    V319 = 319,

    /// <summary>Versão 3.2.0 — incremento sobre V319.</summary>
    V320 = 320,

    /// <summary>Versão 3.2.1 — incremento sobre V320.</summary>
    V321 = 321,

    /// <summary>Versão 3.2.2 — incremento sobre V321 e versão vigente.</summary>
    V322 = 322,
}

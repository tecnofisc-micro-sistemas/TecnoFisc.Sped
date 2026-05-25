namespace TecnoFisc.Sped.Ecd.Versionamento;

/// <summary>
/// Versão do leiaute da ECD (Escrituração Contábil Digital — Sped Contábil) coberta pelo
/// pacote. Atualmente existe um único leiaute dentro da janela fiscal de 5 anos.
/// </summary>
/// <remarks>
/// <para>
/// <b>Leiaute único.</b> O <b>leiaute 9</b> está vigente desde o ano-calendário 2020
/// (Manual de Orientação anexo ao Ato Declaratório Executivo Cofis nº 01/2026,
/// atualização maio/2026). A Receita não publicou leiaute posterior — não há incrementos.
/// </para>
/// <para>
/// <b>Versão fora do <c>0000</c>.</b> Diferente do EFD, o <c>Registro0000</c> da ECD
/// <b>não</b> carrega <c>COD_VER</c>: o campo 02 do <c>0000</c> é o literal <c>"LECD"</c>.
/// O código da versão do leiaute contábil é informado em <c>I010.COD_VER_LC</c>
/// (valor <c>"9.00"</c> para o ano-calendário 2024 em diante). Esta enum modela o leiaute
/// para auditoria/consumidor; em pacote read-only o parser não a usa para filtrar registros
/// (ARCHITECTURE §4.7).
/// </para>
/// <para>
/// <b>Convenção numérica.</b> O valor inteiro da constante é o número do leiaute:
/// <c>V009 = 9</c>.
/// </para>
/// </remarks>
public enum LayoutEcd
{
    /// <summary>Leiaute 9 — vigente a partir do ano-calendário 2020.</summary>
    V009 = 9,
}

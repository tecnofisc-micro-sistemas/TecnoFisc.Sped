using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Gerador;

/// <summary>
/// Injeta os registros de fechamento de bloco (X990) ao final de cada bloco e o registro
/// de fechamento do arquivo (9999) ao final da sequência. As contagens seguem a convenção
/// SPED: o X990 inclui ele próprio na contagem do bloco; o 9999 inclui ele próprio na
/// contagem total do arquivo.
/// </summary>
/// <remarks>
/// O totalizador é format-agnostic: o chamador fornece <c>fabricaX990</c> e <c>fabrica9999</c>
/// que sabem instanciar os tipos concretos do projeto de formato (ex.: <c>RegistroC990</c>
/// para EFD Contribuições). Os registros de entrada não devem conter X990 ou 9999 — caso
/// contrário serão tratados como conteúdo e produzirão dupla totalização.
/// </remarks>
public static class TotalizadorBlocos
{
    /// <summary>
    /// Enumera <paramref name="registros"/> e intercala os fechadores. A enumeração é
    /// preguiçosa: nada é materializado até o chamador iterar.
    /// </summary>
    /// <param name="registros">Sequência de registros do arquivo, sem X990 nem 9999.</param>
    /// <param name="catalogo">Catálogo que resolve o bloco de cada registro.</param>
    /// <param name="fabricaX990">Função que constrói o X990 dado o identificador de bloco e a quantidade total de linhas do bloco (incluindo o próprio X990).</param>
    /// <param name="fabrica9999">Função que constrói o 9999 dado o total de linhas do arquivo (incluindo o próprio 9999).</param>
    public static IEnumerable<RegistroSped> ComFechadores(
        IEnumerable<RegistroSped> registros,
        IRegistroSpedCatalogo catalogo,
        Func<string, int, RegistroSped> fabricaX990,
        Func<int, RegistroSped> fabrica9999)
    {
        ArgumentNullException.ThrowIfNull(registros);
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(fabricaX990);
        ArgumentNullException.ThrowIfNull(fabrica9999);

        return Iterar(registros, catalogo, fabricaX990, fabrica9999);
    }

    private static IEnumerable<RegistroSped> Iterar(
        IEnumerable<RegistroSped> registros,
        IRegistroSpedCatalogo catalogo,
        Func<string, int, RegistroSped> fabricaX990,
        Func<int, RegistroSped> fabrica9999)
    {
        string? blocoCorrente = null;
        int contagemBloco = 0;
        int totalArquivo = 0;

        foreach (var registro in registros)
        {
            if (registro is null)
                throw new InvalidOperationException("Sequência de registros contém um nulo.");

            if (!catalogo.TentarObter(registro.Codigo.AsSpan(), out var metadados))
                throw new InvalidOperationException(
                    $"Registro com código '{registro.Codigo}' não encontrado no catálogo.");

            if (blocoCorrente is not null
                && !metadados.Bloco.Equals(blocoCorrente, StringComparison.Ordinal))
            {
                // Encerrou bloco anterior — emite o X990 com a contagem incluindo o próprio.
                var fechador = fabricaX990(blocoCorrente, contagemBloco + 1);
                yield return fechador;
                totalArquivo++;
                contagemBloco = 0;
            }

            blocoCorrente = metadados.Bloco;
            yield return registro;
            contagemBloco++;
            totalArquivo++;
        }

        if (blocoCorrente is not null)
        {
            var ultimoFechador = fabricaX990(blocoCorrente, contagemBloco + 1);
            yield return ultimoFechador;
            totalArquivo++;
        }

        // 9999 conta a si mesmo: total + 1.
        yield return fabrica9999(totalArquivo + 1);
    }
}

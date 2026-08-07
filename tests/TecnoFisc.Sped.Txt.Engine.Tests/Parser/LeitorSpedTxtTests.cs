using System.Text;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    private static async Task<List<RegistroSped>> LerTodosAsync(string conteudo)
        => await LerComOpcoesAsync(conteudo, ReadingOptions.Default);

    private static async Task<List<RegistroSped>> LerComOpcoesAsync(string conteudo, ReadingOptions opcoes)
    {
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);
        var resultado = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(FluxoSped(conteudo)))
            resultado.Add(registro);
        return resultado;
    }

    private const string ArquivoComHierarquia =
        "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
        "|C001|0|\r\n" +
        "|C100|0|123|1500,75|5102|\r\n" +
        "|C170|1|MERCADORIA A|10|750,50|\r\n" +
        "|C170|2|MERCADORIA B|5|750,25|\r\n" +
        "|C100|0|456|2000,00|6101|\r\n" +
        "|C990|2|\r\n" +
        "|9999|8|\r\n";

    [Fact]
    public async Task LerStreamingAsync_RegistrosIgnorados_DescartaRegistroEToda_Subarvore()
    {
        // Ignorar C100 deve descartar os dois C100 (nível 2) e os C170 filhos (nível 3),
        // mantendo irmãos/fechadores de nível menor-ou-igual (C001, C990).
        var opcoes = new ReadingOptions { RegistrosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "C100" } };

        var registros = await LerComOpcoesAsync(ArquivoComHierarquia, opcoes);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "C990", "9999"]);
    }

    [Fact]
    public async Task LerStreamingAsync_BlocosIgnorados_DescartaTodoOBloco()
    {
        // Ignorar o bloco C descarta abertura (C001), detalhe (C100/C170) e fechamento (C990).
        var opcoes = new ReadingOptions { BlocosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "C" } };

        var registros = await LerComOpcoesAsync(ArquivoComHierarquia, opcoes);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "9999"]);
    }

    [Fact]
    public async Task LerStreamingAsync_SemFiltro_LeTudo()
    {
        // ReadingOptions.Default não filtra nada (fast-path) — comportamento idêntico ao padrão.
        var registros = await LerComOpcoesAsync(ArquivoComHierarquia, ReadingOptions.Default);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "C100", "C170", "C170", "C100", "C990", "9999"]);
    }

    [Fact]
    public async Task LerStreamingAsync_VigenciaDesativada_MantemMetadadosInformacionais()
    {
        const string sped =
            "|0000|309|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|Z100|BASE|FUTURO|\r\n" +
            "|9999|4|\r\n";

        var registros = await LerTodosAsync(sped);

        registros.OfType<RegistroZ100Sintetico>().Single().IndComplemento.Should().Be("FUTURO");
    }

    [Fact]
    public async Task LerStreamingAsync_VigenciaAtivada_OmiteRegistroAntesDaIntroducao()
    {
        const string sped =
            "|0000|309|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|Z100|BASE|\r\n" +
            "|C170|1|ITEM|1|1,00|\r\n" +
            "|9999|5|\r\n";
        var opcoes = new ReadingOptions { RespeitarVigenciaDoLeiaute = true };

        var registros = await LerComOpcoesAsync(sped, opcoes);

        registros.Select(registro => registro.Codigo).Should().Equal("0000", "C001", "9999");
    }

    [Theory]
    [InlineData("310", null)]
    [InlineData("311", null)]
    [InlineData("312", "FUTURO")]
    public async Task LerStreamingAsync_VigenciaAtivada_AtribuiCampoSomenteDesdeSuaVersao(
        string versao,
        string? complementoEsperado)
    {
        string sped =
            $"|0000|{versao}|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|Z100|BASE|FUTURO|\r\n" +
            "|9999|4|\r\n";
        var opcoes = new ReadingOptions { RespeitarVigenciaDoLeiaute = true };

        var registros = await LerComOpcoesAsync(sped, opcoes);

        var registro = registros.OfType<RegistroZ100Sintetico>().Single();
        registro.CodConf.Should().Be("BASE");
        registro.IndComplemento.Should().Be(complementoEsperado);
    }

    [Fact]
    public async Task LerStreamingAsync_VigenciaAtivada_IndexaPelosCamposAtivos()
    {
        // Z200 encadeia dois campos versionados no fim do registro (Futuro desde 312, Depois
        // desde 400) — é a única forma que CatalogoBuilder.ValidarVigenciaCrescente aceita hoje
        // (DesdeVersao não-decrescente ao longo da posição; um campo versionado só pode ficar no
        // fim, nunca seguido por um sempre-presente ou por um de versão anterior). No arquivo de
        // versão 312, Futuro já vigora mas Depois ainda não — mesmo com a coluna de Depois
        // fisicamente preenchida (não vazia, para provar que é a guarda de vigência CampoAtivo,
        // e não a coincidência de string vazia → null, que bloqueia a atribuição). Prova que o
        // mapeamento posicional (indice = posicaoCampo - 2) indexa dois portões de vigência
        // independentes no mesmo registro sem misturar as colunas.
        const string sped =
            "|0000|312|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|Z200|ANTES|FUTURO|DEPOIS|\r\n" +
            "|9999|4|\r\n";
        var opcoes = new ReadingOptions { RespeitarVigenciaDoLeiaute = true };

        var registro = (await LerComOpcoesAsync(sped, opcoes))
            .OfType<RegistroZ200Sintetico>().Single();

        registro.Antes.Should().Be("ANTES");
        registro.Futuro.Should().Be("FUTURO");
        registro.Depois.Should().BeNull();
    }

    [Fact]
    public async Task LerStreamingAsync_RegistroMultilinhaIgnorado_NaoMaterializa()
    {
        // Y800 (multi-linha) ignorado: o leitor localiza o terminador e avança sem decodificar o
        // ARQ_RTF; os registros ao redor continuam sendo lidos.
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|Y800|010|D|H|linha1 do rtf\r\nlinha2 do rtf\r\nfim}|Y800FIM|\r\n" +
            "|C001|0|\r\n" +
            "|9999|4|\r\n";
        var opcoes = new ReadingOptions { RegistrosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "Y800" } };

        var registros = await LerComOpcoesAsync(sped, opcoes);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "9999"]);
        registros.OfType<RegistroY800Sintetico>().Should().BeEmpty();
    }

    [Fact]
    public async Task LerStreamingAsync_BlocoMultilinhaIgnorado_DescartaSemDecodificar()
    {
        // Bloco Y (do Y800 multi-linha) ignorado por bloco.
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|Y800|010|D|H|conteudo\r\nrtf|Y800FIM|\r\n" +
            "|9999|3|\r\n";
        var opcoes = new ReadingOptions { BlocosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "Y" } };

        var registros = await LerComOpcoesAsync(sped, opcoes);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "9999"]);
    }

    [Fact]
    public async Task LerStreamingAsync_ArquivoSimples_MaterializaRegistrosNaOrdem()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA TESTE|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|9999|3|\r\n";

        var registros = await LerTodosAsync(sped);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "C100", "9999"]);

        var r0000 = registros.OfType<Registro0000Sintetico>().Single();
        r0000.CodVer.Should().Be("006");
        r0000.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        r0000.DtFin.Should().Be(new DateOnly(2025, 1, 31));
        r0000.Nome.Should().Be("EMPRESA TESTE");
        r0000.Cnpj.ToString().Should().Be("11222333000181");

        var r9999 = registros.OfType<Registro9999Sintetico>().Single();
        r9999.QtdLin.Should().Be(3);
    }

    [Fact]
    public async Task LerStreamingAsync_VinculaPaiEFilhosViaPilhaHierarquica()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|0|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|C170|1|MERCADORIA A|10|750,50|\r\n" +
            "|C170|2|MERCADORIA B|5|750,25|\r\n" +
            "|C100|0|456|2000,00|6101|\r\n" +
            "|9999|6|\r\n";

        var registros = await LerTodosAsync(sped);

        var r0000 = registros.OfType<Registro0000Sintetico>().Single();
        var c001 = registros.OfType<RegistroC001Sintetico>().Single();
        var c100s = registros.OfType<RegistroC100Sintetico>().ToList();
        var c170s = registros.OfType<RegistroC170Sintetico>().ToList();
        var r9999 = registros.OfType<Registro9999Sintetico>().Single();

        c001.Pai.Should().BeSameAs(r0000);
        c100s[0].Pai.Should().BeSameAs(c001);
        c100s[1].Pai.Should().BeSameAs(c001);
        c170s[0].Pai.Should().BeSameAs(c100s[0]);
        c170s[1].Pai.Should().BeSameAs(c100s[0]);
        // 9999 está no mesmo nível de 0000 (raiz/fechador): ambos são raízes, não pai/filho.
        r9999.Pai.Should().BeNull();

        r0000.Filhos.Should().Equal([c001]);
        r9999.Filhos.Should().BeEmpty();
        c001.Filhos.Should().Equal(c100s);
        c100s[0].Filhos.Should().Equal(c170s);
        c100s[1].Filhos.Should().BeEmpty();
    }

    [Fact]
    public async Task LerStreamingAsync_AceitaTerminadorSomenteLF()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\n" +
            "|9999|2|\n";

        var registros = await LerTodosAsync(sped);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "9999"]);
    }

    [Fact]
    public async Task LerStreamingAsync_AceitaUltimaLinhaSemTerminador()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|9999|2|";

        var registros = await LerTodosAsync(sped);

        registros.Should().HaveCount(2);
    }

    [Fact]
    public async Task LerStreamingAsync_IgnoraLinhasVazias()
    {
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "\r\n" +
            "|9999|2|\r\n";

        var registros = await LerTodosAsync(sped);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "9999"]);
    }

    [Fact]
    public async Task LerStreamingAsync_QuandoCodigoDesconhecido_LancaErroLayout()
    {
        const string sped = "|XXXX|1|\r\n";

        var act = async () => await LerTodosAsync(sped);

        var assercao = await act.Should().ThrowAsync<ErroLayoutSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("XXXX");
        assercao.Which.Erro.Linha.Should().Be(1);
    }

    [Fact]
    public async Task LerStreamingAsync_QuandoSemPipeInicial_LancaErroFormato()
    {
        const string sped = "0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n";

        var act = async () => await LerTodosAsync(sped);

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public async Task LerStreamingAsync_QuandoCampoNumericoMalformado_LancaErroFormato()
    {
        const string sped = "|C001|abc|\r\n";

        var act = async () => await LerTodosAsync(sped);

        var assercao = await act.Should().ThrowAsync<ErroFormatoSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("C001");
        assercao.Which.Erro.Campo.Should().Be("IndMov");
    }

    [Fact]
    public async Task LerStreamingAsync_AceitaCaracteresAcentuadosLatin1()
    {
        const string sped =
            "|0000|006|01012025|31012025|AÇÃO LTDA|11222333000181|\r\n" +
            "|9999|2|\r\n";

        var registros = await LerTodosAsync(sped);

        var r0000 = registros.OfType<Registro0000Sintetico>().Single();
        r0000.Nome.Should().Be("AÇÃO LTDA");
    }

    [Fact]
    public async Task LerStreamingAsync_StreamGrandeAlemDoBufferInterno_LeTodasAsLinhas()
    {
        // Gera mais de 8 KB de conteúdo para forçar múltiplas iterações de PipeReader.ReadAsync.
        var construtor = new StringBuilder();
        construtor.AppendLine("|0000|006|01012025|31012025|EMPRESA|11222333000181|");
        construtor.AppendLine("|C001|0|");
        for (int i = 1; i <= 500; i++)
            construtor.AppendLine($"|C100|0|{i}|1000,00|5102|");
        construtor.AppendLine("|9999|503|");

        var registros = await LerTodosAsync(construtor.ToString());

        registros.OfType<RegistroC100Sintetico>().Should().HaveCount(500);
        registros.OfType<RegistroC100Sintetico>().Last().CodPart.Should().Be(500);
    }

    [Fact]
    public async Task LerStreamingAsync_RegistroMultilinha_RemontaCampoArquivoComCrlfInterno()
    {
        // Y800 é multi-linha (TokenFimArquivo="Y800FIM"): ARQ_RTF carrega um arquivo com quebras
        // CRLF internas, ocupando 3 linhas físicas. O leitor deve remontar o registro até a linha
        // que termina em |Y800FIM| e capturar ArqRtf preservando os CRLFs internos.
        const string arqRtf = "{\\rtf1\\ansi\r\nlinha 2 do rtf\r\nfim}";
        const string sped =
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|Y800|010|Notas|HASH123|{\\rtf1\\ansi\r\n" +
            "linha 2 do rtf\r\n" +
            "fim}|Y800FIM|\r\n" +
            "|9999|4|\r\n";

        var registros = await LerTodosAsync(sped);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "Y800", "9999"]);

        var y800 = registros.OfType<RegistroY800Sintetico>().Single();
        y800.TipoDoc.Should().Be("010");
        y800.DescRtf.Should().Be("Notas");
        y800.HashRtf.Should().Be("HASH123");
        y800.ArqRtf.Should().Be(arqRtf);
        y800.IndFimRtf.Should().Be("Y800FIM");
    }

    [Fact]
    public async Task LerStreamingAsync_RegistroMultilinha_CampoArquivoComPipeInterno_PreservaConteudo()
    {
        // ARQ_RTF contém '|' interno — split ingênuo quebraria. O campo-arquivo deve capturar
        // tudo até o último '|' (separador do token de fim), preservando '|' e CRLFs internos.
        const string arqRtf = "linha A|com pipe\r\nlinha B|tambem|com pipes";
        const string sped =
            "|Y800|010|D|H|linha A|com pipe\r\n" +
            "linha B|tambem|com pipes|Y800FIM|\r\n" +
            "|9999|2|\r\n";

        var registros = await LerTodosAsync(sped);

        var y800 = registros.OfType<RegistroY800Sintetico>().Single();
        y800.ArqRtf.Should().Be(arqRtf);
        y800.IndFimRtf.Should().Be("Y800FIM");
    }

    [Fact]
    public async Task LerStreamingAsync_RegistroMultilinhaMaiorQueBufferInterno_RemontaCompleto()
    {
        // ARQ_RTF com milhares de linhas físicas (> buffer do PipeReader) força múltiplas
        // iterações de ReadAsync; o registro deve permanecer bufferizado até o terminador.
        var rtf = new StringBuilder();
        for (int i = 0; i < 2000; i++)
            rtf.Append("conteudo da linha numero ").Append(i).Append("\r\n");
        rtf.Append("FIM");
        string arqRtf = rtf.ToString();

        string sped =
            "|Y800|010|D|H|" + arqRtf + "|Y800FIM|\r\n" +
            "|9999|2|\r\n";

        var registros = await LerTodosAsync(sped);

        var y800 = registros.OfType<RegistroY800Sintetico>().Single();
        y800.ArqRtf.Should().Be(arqRtf);
        registros[^1].Codigo.Should().Be("9999");
    }

    [Fact]
    public async Task LerStreamingAsync_RegistroMultilinhaSemTerminador_LancaErroFormato()
    {
        // Arquivo truncado: registro multi-linha começa mas não há |Y800FIM|.
        const string sped =
            "|Y800|010|D|H|{\\rtf1\r\n" +
            "conteudo sem terminador\r\n";

        var act = async () => await LerTodosAsync(sped);

        var assercao = await act.Should().ThrowAsync<ErroFormatoSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("Y800");
    }

    [Fact]
    public async Task LerStreamingAsync_QuandoStreamNulo_LancaArgumentNullException()
    {
        var leitor = new LeitorSpedTxt(_catalogo);

        var act = async () =>
        {
            await foreach (var _ in leitor.ReadStreamingAsync(null!)) { }
        };

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LerStreamingAsync_QuandoBytesAposEncerramento9999_IgnoraSilenciosamente()
    {
        // Simula arquivo emitido pelo PVA da Receita: registros válidos terminados por |9999|,
        // seguidos de bytes binários da assinatura digital PKCS#7. O parser deve encerrar no
        // |9999| e descartar o resto sem lançar erro de layout/formato.
        var prefixo = EncodingSped.Latin1.GetBytes(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|9999|2|\r\n");
        var lixo = new byte[] { 0x30, 0x82, 0x01, 0xA0, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x02 };
        var combinado = new byte[prefixo.Length + lixo.Length];
        Buffer.BlockCopy(prefixo, 0, combinado, 0, prefixo.Length);
        Buffer.BlockCopy(lixo, 0, combinado, prefixo.Length, lixo.Length);

        var leitor = new LeitorSpedTxt(_catalogo);
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(new MemoryStream(combinado), TestContext.Current.CancellationToken))
            registros.Add(registro);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "9999"]);
    }

}

using System.Text;

using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.NFeNFCe;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Benchmark inicial da slice 14.3: mede o custo de <see cref="ParserNFe.ReadNFeAsync"/> sobre
/// uma NF-e 55 (ICMS60) em dois formatos — o XML canônico (<c>nfeProc</c>) e o envelope SERPRO
/// (<c>NFeLog</c> com elementos reordenados). A tese de projeto (<c>STAGE_14</c> §3) é que a
/// desserialização order-independent lê os dois formatos na mesma velocidade; este benchmark
/// estabelece a linha de base para verificá-la e detectar regressões.
/// </summary>
[MemoryDiagnoser]
public class ParserNFeBenchmark
{
    private const string Chave = "31210711222333000181550050007156531436322400";

    private byte[] _canonico = [];
    private byte[] _serpro = [];
    private readonly ParserNFe _parser = new();

    /// <summary>Quantidade de itens (<c>det</c>) na nota sintética.</summary>
    [Params(1, 50, 500)]
    public int QtdItens { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _canonico = Encoding.UTF8.GetBytes(GerarCanonico(QtdItens));
        _serpro = Encoding.UTF8.GetBytes(GerarSerpro(QtdItens));
    }

    [Benchmark(Baseline = true, Description = "Canônico (nfeProc)")]
    public async Task<int> Canonico()
    {
        using var stream = new MemoryStream(_canonico, writable: false);
        var nfe = await _parser.ReadNFeAsync(stream);
        return nfe.Itens.Count;
    }

    [Benchmark(Description = "SERPRO (NFeLog reordenado)")]
    public async Task<int> Serpro()
    {
        using var stream = new MemoryStream(_serpro, writable: false);
        var nfe = await _parser.ReadNFeAsync(stream);
        return nfe.Itens.Count;
    }

    private static string Ide() =>
        "<ide><cUF>31</cUF><cNF>43632240</cNF><natOp>VENDA</natOp><mod>55</mod><serie>5</serie>" +
        "<nNF>715653</nNF><dhEmi>2021-07-19T10:38:40-03:00</dhEmi><tpNF>1</tpNF><idDest>1</idDest>" +
        "<cMunFG>3168606</cMunFG><tpImp>2</tpImp><tpEmis>1</tpEmis><cDV>0</cDV><tpAmb>1</tpAmb>" +
        "<finNFe>1</finNFe><indFinal>0</indFinal><indPres>9</indPres><procEmi>0</procEmi><verProc>1.0</verProc></ide>";

    private static string Emit() =>
        "<emit><CNPJ>11222333000181</CNPJ><xNome>EMPRESA TESTE LTDA</xNome>" +
        "<enderEmit><xLgr>Av. Exemplo</xLgr><nro>110</nro><xBairro>CENTRO</xBairro><cMun>3168606</cMun>" +
        "<xMun>TEOFILO OTONI</xMun><UF>MG</UF></enderEmit><IE>0623079040081</IE><CRT>3</CRT></emit>";

    private static string Dest() =>
        "<dest><CNPJ>11444777000161</CNPJ><xNome>CLIENTE TESTE LTDA</xNome><indIEDest>9</indIEDest></dest>";

    private static string Item(int n) =>
        $"<det nItem=\"{n}\"><prod><cProd>P{n}</cProd><cEAN>SEM GTIN</cEAN><xProd>PRODUTO {n}</xProd>" +
        "<NCM>22030000</NCM><CFOP>5409</CFOP><uCom>UN</uCom><qCom>1.0000</qCom><vUnCom>10.00</vUnCom>" +
        "<vProd>10.00</vProd><cEANTrib>SEM GTIN</cEANTrib><uTrib>UN</uTrib><qTrib>1.0000</qTrib>" +
        "<vUnTrib>10.00</vUnTrib><indTot>1</indTot></prod>" +
        "<imposto><ICMS><ICMS60><orig>0</orig><CST>60</CST><vBCSTRet>0.00</vBCSTRet>" +
        "<vICMSSubstituto>1.80</vICMSSubstituto><vICMSSTRet>0.00</vICMSSTRet></ICMS60></ICMS></imposto></det>";

    private static string Total() =>
        "<total><ICMSTot><vBC>0.00</vBC><vICMS>0.00</vICMS><vProd>10.00</vProd><vNF>10.00</vNF></ICMSTot></total>";

    private static string Prot() =>
        $"<protNFe versao=\"4.00\"><infProt><tpAmb>1</tpAmb><chNFe>{Chave}</chNFe>" +
        "<dhRecbto>2021-07-19T10:38:28-03:00</dhRecbto><nProt>131214250732778</nProt><cStat>100</cStat>" +
        "<xMotivo>Autorizado o uso da NF-e</xMotivo></infProt></protNFe>";

    private static string Itens(int qtd)
    {
        var sb = new StringBuilder();
        for (int i = 1; i <= qtd; i++)
            sb.Append(Item(i));
        return sb.ToString();
    }

    private static string GerarCanonico(int qtd) =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<nfeProc versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\"><NFe>" +
        $"<infNFe Id=\"NFe{Chave}\" versao=\"4.00\">{Ide()}{Emit()}{Dest()}{Itens(qtd)}{Total()}</infNFe>" +
        $"</NFe>{Prot()}</nfeProc>";

    // Envelope SERPRO: protNFe antes da NFe, Id como elemento-filho, sem namespace.
    private static string GerarSerpro(int qtd) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
        "<NFeLog versao=\"1.00\"><nfeProc>" + Prot() +
        $"<NFe><infNFe>{Itens(qtd)}{Total()}{Dest()}{Emit()}{Ide()}<Id>NFe{Chave}</Id></infNFe></NFe>" +
        "<versao>4.00</versao></nfeProc></NFeLog>";
}

using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Auxiliar de teste compartilhado para desserializar snippets ICMS via o parser real.
/// Reutilizado por IcmsNormalTests, IcmsSimplesTests e variantes futuras.
/// </summary>
internal static class IcmsTestHelper
{
    /// <summary>
    /// Envolve um snippet <c>&lt;ICMS&gt;...&lt;/ICMS&gt;</c> dentro de uma NF-e mínima mas
    /// suficiente para o parser não lançar FormatException, e executa a leitura devolvendo o
    /// <see cref="Icms"/> do primeiro item.
    /// </summary>
    /// <param name="icmsSnippet">Conteúdo interno do elemento <c>&lt;ICMS&gt;</c>, por ex. um bloco <c>&lt;ICMS00&gt;…&lt;/ICMS00&gt;</c>.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O <see cref="Icms"/> do primeiro item da NF-e, ou <c>null</c> se o grupo ICMS estiver ausente.</returns>
    public static async Task<Icms?> ParseIcmsAsync(string icmsSnippet, CancellationToken ct)
    {
        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <nfeProc versao="4.00" xmlns="http://www.portalfiscal.inf.br/nfe">
              <NFe xmlns="http://www.portalfiscal.inf.br/nfe">
                <infNFe Id="NFe31210711222333000181550050007156531436322400" versao="4.00">
                  <ide>
                    <cUF>31</cUF>
                    <cNF>43632240</cNF>
                    <natOp>VENDA</natOp>
                    <mod>55</mod>
                    <serie>1</serie>
                    <nNF>1</nNF>
                    <dhEmi>2021-07-19T10:38:40-03:00</dhEmi>
                    <tpNF>1</tpNF>
                    <idDest>1</idDest>
                    <cMunFG>3168606</cMunFG>
                    <tpImp>1</tpImp>
                    <tpEmis>1</tpEmis>
                    <cDV>0</cDV>
                    <tpAmb>2</tpAmb>
                    <finNFe>1</finNFe>
                    <indFinal>0</indFinal>
                    <indPres>1</indPres>
                    <procEmi>0</procEmi>
                    <verProc>1.0</verProc>
                  </ide>
                  <emit>
                    <CNPJ>11222333000181</CNPJ>
                    <xNome>EMPRESA TESTE LTDA</xNome>
                    <enderEmit>
                      <xLgr>Rua Teste</xLgr>
                      <nro>1</nro>
                      <xBairro>Centro</xBairro>
                      <cMun>3168606</cMun>
                      <xMun>TEOFILO OTONI</xMun>
                      <UF>MG</UF>
                    </enderEmit>
                    <CRT>3</CRT>
                  </emit>
                  <det nItem="1">
                    <prod>
                      <cProd>001</cProd>
                      <cEAN>SEM GTIN</cEAN>
                      <xProd>PRODUTO TESTE</xProd>
                      <NCM>22030000</NCM>
                      <CFOP>5102</CFOP>
                      <uCom>UN</uCom>
                      <qCom>1.0000</qCom>
                      <vUnCom>10.00</vUnCom>
                      <vProd>10.00</vProd>
                      <cEANTrib>SEM GTIN</cEANTrib>
                      <uTrib>UN</uTrib>
                      <qTrib>1.0000</qTrib>
                      <vUnTrib>10.00</vUnTrib>
                      <indTot>1</indTot>
                    </prod>
                    <imposto>
                      <ICMS>
                        {icmsSnippet}
                      </ICMS>
                    </imposto>
                  </det>
                  <total>
                    <ICMSTot>
                      <vBC>0.00</vBC>
                      <vICMS>0.00</vICMS>
                      <vICMSDeson>0.00</vICMSDeson>
                      <vFCP>0.00</vFCP>
                      <vBCST>0.00</vBCST>
                      <vST>0.00</vST>
                      <vFCPST>0.00</vFCPST>
                      <vFCPSTRet>0.00</vFCPSTRet>
                      <vProd>10.00</vProd>
                      <vFrete>0.00</vFrete>
                      <vSeg>0.00</vSeg>
                      <vDesc>0.00</vDesc>
                      <vII>0.00</vII>
                      <vIPI>0.00</vIPI>
                      <vIPIDevol>0.00</vIPIDevol>
                      <vPIS>0.00</vPIS>
                      <vCOFINS>0.00</vCOFINS>
                      <vOutro>0.00</vOutro>
                      <vNF>10.00</vNF>
                    </ICMSTot>
                  </total>
                </infNFe>
              </NFe>
            </nfeProc>
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var parser = new ParserNFe();
        var nfe = await parser.ReadNFeAsync(stream, ct);
        return nfe.Itens[0].Imposto.Icms;
    }
}

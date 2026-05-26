using System.Globalization;
using System.Xml;

using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Desserializa uma NF-e (modelo 55) a partir de um <see cref="XmlReader"/> forward-only,
/// de forma <b>order-independent</b>: o entry point varre o documento e colhe o grupo
/// <c>infNFe</c> e o protocolo (<c>infProt</c>) onde quer que apareçam — o que cobre tanto o
/// XML canônico (<c>nfeProc</c>/<c>NFe</c>) quanto o envelope SERPRO (SOAP →
/// <c>retConsNFeLog</c> → <c>NFeLog</c>), que embaralha a ordem dos elementos e inverte
/// <c>protNFe</c>/<c>NFe</c>. Eventos embutidos no envelope SERPRO são ignorados nesta slice
/// (entram na 14.7).
/// </summary>
/// <remarks>
/// Instância de uso único, criada por chamada de <c>ParserNFe.ReadAsync</c> — não compartilha
/// estado mutável entre threads (o estado é o <see cref="XmlReader"/> da própria chamada).
/// A chave de acesso é sempre validada (é a identidade do documento);
/// <see cref="ParserNFeOptions.ValidateChecksums"/> governa os demais value objects com DV (GTIN).
/// </remarks>
internal sealed partial class NFeXmlReader(XmlReader reader, ParserNFeOptions options)
{
    private readonly XmlReader _r = reader;
    private readonly ParserNFeOptions _options = options;

    private sealed record InfNFe(
        ChaveAcesso Chave, string Versao, Identificacao Ide, Emitente Emit,
        Destinatario? Dest, Total Total, IReadOnlyList<Item> Itens);

    /// <summary>Varre o documento e materializa a <see cref="NFe"/> (com protocolo, se houver).</summary>
    public async Task<NFe> ReadAsync(CancellationToken cancellationToken)
    {
        InfNFe? core = null;
        Protocolo? protocolo = null;

        if (!await _r.ReadAsync().ConfigureAwait(false))
            throw new FormatException("Documento XML vazio.");

        while (!_r.EOF)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_r.NodeType != XmlNodeType.Element)
            {
                if (!await _r.ReadAsync().ConfigureAwait(false))
                    break;
                continue;
            }

            switch (_r.LocalName)
            {
                case "infNFe":
                    if (core is null)
                        core = await ReadInfNFeAsync(cancellationToken).ConfigureAwait(false);
                    else
                        await _r.SkipAsync().ConfigureAwait(false);
                    break; // leitor/Skip já deixou o reader no nó seguinte

                case "infProt":
                    if (protocolo is null)
                        protocolo = await ReadInfProtAsync(cancellationToken).ConfigureAwait(false);
                    else
                        await _r.SkipAsync().ConfigureAwait(false);
                    break;

                default:
                    // Desce em qualquer envelope (soap/NFeLog/nfeProc/NFe/Signature/eventos)
                    // procurando os nós de interesse.
                    if (!await _r.ReadAsync().ConfigureAwait(false))
                        goto done;
                    break;
            }
        }

    done:
        if (core is null)
            throw new FormatException("Nenhum grupo infNFe encontrado no documento.");

        return new NFe
        {
            ChaveAcesso = core.Chave,
            Versao = core.Versao,
            Ide = core.Ide,
            Emit = core.Emit,
            Dest = core.Dest,
            Total = core.Total,
            Itens = core.Itens,
            Protocolo = protocolo,
        };
    }

    private async Task<InfNFe> ReadInfNFeAsync(CancellationToken ct)
    {
        // Id e versao podem vir como atributo (canônico) ou elemento-filho (SERPRO).
        string? id = _r.GetAttribute("Id");
        string? versao = _r.GetAttribute("versao");

        Identificacao? ide = null;
        Emitente? emit = null;
        Destinatario? dest = null;
        Total? total = null;
        var itens = new List<Item>();

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ide": ide = await ReadIdeAsync(ct).ConfigureAwait(false); return true;
                case "emit": emit = await ReadEmitenteAsync(ct).ConfigureAwait(false); return true;
                case "dest": dest = await ReadDestinatarioAsync(ct).ConfigureAwait(false); return true;
                case "total": total = await ReadTotalAsync(ct).ConfigureAwait(false); return true;
                case "det": itens.Add(await ReadItemAsync(ct).ConfigureAwait(false)); return true;
                // Lê sempre (consome o elemento), mas o atributo homônimo tem precedência se já veio.
                case "Id": { var v = await _r.ReadTextAsync().ConfigureAwait(false); id ??= v; return true; }
                case "versao": { var v = await _r.ReadTextAsync().ConfigureAwait(false); versao ??= v; return true; }
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new InfNFe(
            ChaveFromId(id),
            versao ?? string.Empty,
            ide ?? throw new FormatException("infNFe sem grupo ide."),
            emit ?? throw new FormatException("infNFe sem grupo emit."),
            dest,
            total ?? throw new FormatException("infNFe sem grupo total."),
            itens);
    }

    private async Task<Identificacao> ReadIdeAsync(CancellationToken ct)
    {
        int cUF = 0; string cNF = string.Empty; string natOp = string.Empty; ModeloDocumento mod = default;
        int serie = 0; int nNF = 0; DateTimeOffset dhEmi = default; DateTimeOffset? dhSaiEnt = null;
        IndicadorOperacao tpNF = default; int idDest = 0; CodigoMunicipioIbge cMunFG = default;
        int tpImp = 0; TipoEmissao tpEmis = default; int cDV = 0; TipoAmbiente tpAmb = default;
        FinalidadeEmissao finNFe = default; bool indFinal = false; IndicadorPresenca indPres = default;
        IndicadorIntermediador? indIntermed = null; int procEmi = 0; string? verProc = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cUF": cUF = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cNF": cNF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "natOp": natOp = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "mod": mod = ModeloDocumento.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "serie": serie = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "nNF": nNF = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "dhEmi": dhEmi = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "dhSaiEnt": dhSaiEnt = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpNF": tpNF = ParseEnum<IndicadorOperacao>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "idDest": idDest = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cMunFG": cMunFG = CodigoMunicipioIbge.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpImp": tpImp = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpEmis": tpEmis = ParseEnum<TipoEmissao>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cDV": cDV = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "tpAmb": tpAmb = ParseEnum<TipoAmbiente>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "finNFe": finNFe = ParseEnum<FinalidadeEmissao>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indFinal": indFinal = ParseBool(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indPres": indPres = ParseEnum<IndicadorPresenca>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indIntermed": indIntermed = ParseEnum<IndicadorIntermediador>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "procEmi": procEmi = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "verProc": verProc = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Identificacao
        {
            CUF = cUF, CNF = cNF, NatOp = natOp, Mod = mod, Serie = serie, NNF = nNF,
            DhEmi = dhEmi, DhSaiEnt = dhSaiEnt, TpNF = tpNF, IdDest = idDest, CMunFG = cMunFG,
            TpImp = tpImp, TpEmis = tpEmis, CDV = cDV, TpAmb = tpAmb, FinNFe = finNFe,
            IndFinal = indFinal, IndPres = indPres, IndIntermed = indIntermed,
            ProcEmi = procEmi, VerProc = verProc,
        };
    }

    private async Task<Emitente> ReadEmitenteAsync(CancellationToken ct)
    {
        Cnpj? cnpj = null; Cpf? cpf = null; string xNome = string.Empty; string? xFant = null;
        Endereco? ender = null; InscricaoEstadual? ie = null; string? iest = null, im = null, cnae = null;
        int crt = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": cnpj = Cnpj.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CPF": cpf = Cpf.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xNome": xNome = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xFant": xFant = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "enderEmit": ender = await ReadEnderecoAsync(ct).ConfigureAwait(false); return true;
                case "IE": ie = InscricaoEstadual.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "IEST": iest = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "IM": im = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CNAE": cnae = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CRT": crt = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Emitente
        {
            CNPJ = cnpj, CPF = cpf, XNome = xNome, XFant = xFant,
            EnderEmit = ender ?? throw new FormatException("emit sem grupo enderEmit."),
            IE = ie, IEST = iest, IM = im, CNAE = cnae, CRT = crt,
        };
    }

    private async Task<Destinatario> ReadDestinatarioAsync(CancellationToken ct)
    {
        Cnpj? cnpj = null; Cpf? cpf = null; string? idEstrangeiro = null, xNome = null, email = null;
        Endereco? ender = null; int? indIEDest = null; InscricaoEstadual? ie = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": cnpj = Cnpj.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CPF": cpf = Cpf.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "idEstrangeiro": idEstrangeiro = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xNome": xNome = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "enderDest": ender = await ReadEnderecoAsync(ct).ConfigureAwait(false); return true;
                case "indIEDest": indIEDest = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "IE": ie = InscricaoEstadual.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "email": email = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Destinatario
        {
            CNPJ = cnpj, CPF = cpf, IdEstrangeiro = idEstrangeiro, XNome = xNome,
            EnderDest = ender, IndIEDest = indIEDest, IE = ie, Email = email,
        };
    }

    private async Task<Endereco> ReadEnderecoAsync(CancellationToken ct)
    {
        string xLgr = string.Empty, nro = string.Empty, xBairro = string.Empty, xMun = string.Empty, uf = string.Empty;
        string? xCpl = null, cep = null, xPais = null, fone = null;
        CodigoMunicipioIbge cMun = default; int? cPais = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xLgr": xLgr = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nro": nro = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xCpl": xCpl = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xBairro": xBairro = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cMun": cMun = CodigoMunicipioIbge.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xMun": xMun = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "UF": uf = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CEP": cep = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cPais": cPais = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xPais": xPais = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "fone": fone = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Endereco
        {
            XLgr = xLgr, Nro = nro, XCpl = xCpl, XBairro = xBairro, CMun = cMun,
            XMun = xMun, UF = uf, CEP = cep, CPais = cPais, XPais = xPais, Fone = fone,
        };
    }

    private async Task<Total> ReadTotalAsync(CancellationToken ct)
    {
        TotalIcms? icmsTot = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ICMSTot": icmsTot = await ReadTotalIcmsAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Total { ICMSTot = icmsTot ?? throw new FormatException("total sem grupo ICMSTot.") };
    }

    private async Task<TotalIcms> ReadTotalIcmsAsync(CancellationToken ct)
    {
        decimal vBC = 0, vICMS = 0, vICMSDeson = 0, vFCP = 0, vBCST = 0, vST = 0, vFCPST = 0,
            vFCPSTRet = 0, vProd = 0, vFrete = 0, vSeg = 0, vDesc = 0, vII = 0, vIPI = 0,
            vIPIDevol = 0, vPIS = 0, vCOFINS = 0, vOutro = 0, vNF = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vST": vST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPSTRet": vFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vProd": vProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFrete": vFrete = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vSeg": vSeg = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vDesc": vDesc = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vII": vII = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vIPI": vIPI = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vIPIDevol": vIPIDevol = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vPIS": vPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCOFINS": vCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vOutro": vOutro = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vNF": vNF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new TotalIcms
        {
            VBC = vBC, VICMS = vICMS, VICMSDeson = vICMSDeson, VFCP = vFCP, VBCST = vBCST,
            VST = vST, VFCPST = vFCPST, VFCPSTRet = vFCPSTRet, VProd = vProd, VFrete = vFrete,
            VSeg = vSeg, VDesc = vDesc, VII = vII, VIPI = vIPI, VIPIDevol = vIPIDevol,
            VPIS = vPIS, VCOFINS = vCOFINS, VOutro = vOutro, VNF = vNF,
        };
    }

    private async Task<Item> ReadItemAsync(CancellationToken ct)
    {
        // nItem vem como atributo (canônico) ou elemento-filho (SERPRO).
        string? nItemAttr = _r.GetAttribute("nItem");
        int nItem = nItemAttr is null ? 0 : ParseInt(nItemAttr);

        Produto? prod = null;
        var imposto = new Imposto();

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nItem": nItem = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "prod": prod = await ReadProdutoAsync(ct).ConfigureAwait(false); return true;
                case "imposto": imposto = await ReadImpostoAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Item
        {
            NItem = nItem,
            Prod = prod ?? throw new FormatException($"det (nItem={nItem}) sem grupo prod."),
            Imposto = imposto,
        };
    }

    private async Task<Produto> ReadProdutoAsync(CancellationToken ct)
    {
        string cProd = string.Empty, xProd = string.Empty, uCom = string.Empty, uTrib = string.Empty;
        string? extipi = null;
        Gtin cEAN = default, cEANTrib = default; Ncm ncm = default; Cest? cest = null; Cfop cfop = default;
        decimal qCom = 0, vUnCom = 0, vProd = 0, qTrib = 0, vUnTrib = 0; bool indTot = false;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cProd": cProd = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cEAN": cEAN = ParseGtin(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xProd": xProd = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "NCM": ncm = Ncm.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CEST": cest = Cest.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "EXTIPI": extipi = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CFOP": cfop = Cfop.Create(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "uCom": uCom = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qCom": qCom = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vUnCom": vUnCom = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vProd": vProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cEANTrib": cEANTrib = ParseGtin(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "uTrib": uTrib = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qTrib": qTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vUnTrib": vUnTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indTot": indTot = ParseBool(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Produto
        {
            CProd = cProd, CEAN = cEAN, XProd = xProd, NCM = ncm, CEST = cest, EXTIPI = extipi,
            CFOP = cfop, UCom = uCom, QCom = qCom, VUnCom = vUnCom, VProd = vProd,
            CEANTrib = cEANTrib, UTrib = uTrib, QTrib = qTrib, VUnTrib = vUnTrib, IndTot = indTot,
        };
    }

    private async Task<Imposto> ReadImpostoAsync(CancellationToken ct)
    {
        Icms? icms = null;
        Ipi? ipi = null;
        Pis? pis = null;
        PisSt? pisSt = null;
        Cofins? cofins = null;
        CofinsSt? cofinsSt = null;
        Ii? ii = null;
        Issqn? issqn = null;
        decimal? vTotTrib = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ICMS": icms = await ReadIcmsAsync(ct).ConfigureAwait(false); return true;
                case "IPI": ipi = await ReadIpiAsync(ct).ConfigureAwait(false); return true;
                case "PIS": pis = await ReadPisAsync(ct).ConfigureAwait(false); return true;
                case "PISST": pisSt = await ReadPisStAsync(ct).ConfigureAwait(false); return true;
                case "COFINS": cofins = await ReadCofinsAsync(ct).ConfigureAwait(false); return true;
                case "COFINSST": cofinsSt = await ReadCofinsStAsync(ct).ConfigureAwait(false); return true;
                case "II": ii = await ReadIiAsync(ct).ConfigureAwait(false); return true;
                case "ISSQN": issqn = await ReadIssqnAsync(ct).ConfigureAwait(false); return true;
                case "vTotTrib": vTotTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Imposto { Icms = icms, Ipi = ipi, Pis = pis, PisSt = pisSt, Cofins = cofins, CofinsSt = cofinsSt, Ii = ii, Issqn = issqn, VTotTrib = vTotTrib };
    }

    private async Task<Protocolo> ReadInfProtAsync(CancellationToken ct)
    {
        TipoAmbiente tpAmb = default; string? verAplic = null; ChaveAcesso chNFe = default; bool chSet = false;
        DateTimeOffset dhRecbto = default; string? nProt = null, digVal = null, xMotivo = null; int cStat = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpAmb": tpAmb = ParseEnum<TipoAmbiente>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "verAplic": verAplic = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "chNFe": chNFe = ChaveAcesso.Create(await _r.ReadTextAsync().ConfigureAwait(false)); chSet = true; return true;
                case "dhRecbto": dhRecbto = ParseDate(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "nProt": nProt = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "digVal": digVal = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cStat": cStat = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "xMotivo": xMotivo = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                // No SERPRO, o protocolo traz o nº como elemento <Id> (não é a chave): consome e descarta.
                case "Id": await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Protocolo
        {
            TpAmb = tpAmb, VerAplic = verAplic,
            ChNFe = chSet ? chNFe : throw new FormatException("infProt sem chNFe."),
            DhRecbto = dhRecbto, NProt = nProt, DigVal = digVal, CStat = cStat, XMotivo = xMotivo,
        };
    }

    /// <summary>Converte o atributo/elemento <c>Id</c> ("NFe" + 44 dígitos) em <see cref="ChaveAcesso"/>.</summary>
    private static ChaveAcesso ChaveFromId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new FormatException("infNFe sem atributo/elemento Id — chave de acesso indisponível.");

        ReadOnlySpan<char> s = id.AsSpan().Trim();
        if (s.StartsWith("NFe", StringComparison.OrdinalIgnoreCase))
            s = s[3..];

        return ChaveAcesso.Create(s);
    }

    /// <summary>Constrói um <see cref="Gtin"/> respeitando <see cref="ParserNFeOptions.ValidateChecksums"/>.</summary>
    private Gtin ParseGtin(string valor)
        => _options.ValidateChecksums
            ? Gtin.Create(valor)
            : Gtin.TentarCriar(valor, out var g) ? g : default;

    /// <summary>
    /// Combina <c>orig</c> e o <c>CST</c> de 2 dígitos da NF-e na forma canônica SPED/Ato COTEPE
    /// de 3 dígitos (origem + CST) do value object <see cref="Cst"/> do Core.
    /// </summary>
    private static Cst CombinarCst(OrigemMercadoria orig, string cstTributacao)
        => Cst.Create(string.Concat(((int)orig).ToString(CultureInfo.InvariantCulture), cstTributacao), TipoTributo.Icms);

    private static int ParseInt(string s) => int.Parse(s, CultureInfo.InvariantCulture);

    private static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);

    private static decimal ParseDecimal(string s) => decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseDate(string s)
        => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static bool ParseBool(string s) => s == "1";

    private static T ParseEnum<T>(string s) where T : struct, Enum
        => (T)Enum.ToObject(typeof(T), ParseInt(s));
}

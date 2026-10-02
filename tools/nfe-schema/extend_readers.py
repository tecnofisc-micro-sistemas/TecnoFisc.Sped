"""One-shot, offline scaffolding of missing typed fields. Review emitted C#.

Reads the original reader switches to preserve semantic conversions and API names.
No generator, reflection, dictionaries, or schema processing runs in the library.
"""
import json
import re
from pathlib import Path
from inventory import Schema, XS

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / 'src/TecnoFisc.Sped.NFeNFCe'
if (SRC/'Modelos/Complementos').exists():
    raise SystemExit('One-shot scaffold already applied. Edit/review the committed C#; do not overwrite it.')
schema = Schema(ROOT / 'sped/nfe/schemas')
files = {p: p.read_text(encoding='utf-8-sig') for p in (SRC/'Parser').glob('NFeXmlReader*.cs') if 'Complementos' not in p.name}
methods = {}
for p, text in files.items():
    for m in re.finditer(r'    private async Task<([^>]+)> (Read\w+Async)\(CancellationToken ct\)\n    \{', text):
        end = text.find('\n    }', m.end()) + 6
        methods[m[2]] = dict(type=m[1].rstrip('?'), file=p, old=text[m.start():end], text=text[m.start():end])

names = {'transp':'Transporte','cobr':'Cobranca','pag':'Pagamento','infAdic':'InformacoesAdicionais',
         'infRespTec':'ResponsavelTecnico','autXML':'AutorizadoXml','retirada':'LocalRetirada','entrega':'LocalEntrega',
         'infIntermed':'Intermediador','compra':'Compra','exporta':'Exportacao','cana':'Cana','NFref':'ReferenciaNota',
         'DI':'DeclaracaoImportacao','detExport':'DetalheExportacao','rastro':'Rastreabilidade','med':'Medicamento',
         'veicProd':'VeiculoProduto','arma':'Armamento','comb':'Combustivel','IBSCBS':'TributacaoIBSCBS','IS':'ImpostoSeletivo',
         'ICMSUFDest':'IcmsUfDest','ISSQNtot':'TotalIssqn','retTrib':'RetencoesTributos','IBSCBSTot':'TotalIBSCBS',
         'ISTot':'TotalIS','infNFeSupl':'InformacaoSuplementar'}
types_used = {p.stem for p in (SRC/'Modelos').glob('*.cs')}
models = {}
new_methods = {}
additions = {}
bindings = []
visited = set()

def pascal(s): return s[0].upper()+s[1:]

def cs_type(field):
    base = schema.base(field['node'])
    return {'decimal':'decimal', 'date':'DateOnly', 'dateTime':'DateTimeOffset', 'integer':'long',
            'int':'int', 'long':'long','nonNegativeInteger':'long','positiveInteger':'long','boolean':'bool'}.get(base, 'string')

def model_name(field, parent):
    name = field['name']
    if name in ('ICMS02','ICMS15','ICMS53','ICMS61'): return 'Icms'+name[4:]
    proposed = names.get(name, parent+pascal(name))
    if proposed in types_used:
        proposed = parent+pascal(name)
    types_used.add(proposed)
    return proposed

def field_def(field, parent, path):
    node = field['node']
    prop = pascal(field['name'])
    many = str(field['max']) != '1'
    complex_type = schema.type_node(node)
    if complex_type is not None and complex_type.tag == XS+'complexType':
        typ = model_name(field, parent)
        method = 'Read'+typ+'Async'
        build_new(typ, method, complex_type, path)
        expr = f'await {method}(ct).ConfigureAwait(false)'
    else:
        typ = cs_type(field)
        expr = convert(typ, 'await _r.ReadTextAsync().ConfigureAwait(false)')
    return dict(prop=prop, typ=typ, many=many, expr=expr, name=field['name'], attribute=field['attribute'], path=path)

def convert(typ, text):
    if typ == 'string': return text
    if typ == 'decimal': return f'ParseDecimal({text})'
    if typ in ('long','int'): return f'{"ParseLong" if typ == "long" else "ParseInt"}({text})'
    if typ == 'DateOnly': return f'DateOnly.Parse({text}, CultureInfo.InvariantCulture)'
    if typ == 'DateTimeOffset': return f'ParseDate({text})'
    if typ == 'bool': return f'XmlConvert.ToBoolean({text})'
    raise ValueError(typ)

def prop_decl(f):
    return f'    /// <summary>XML: <c>{f["name"]}</c>.</summary>\n    public '+(f'IReadOnlyList<{f["typ"]}> {f["prop"]} {{ get; init; }} = [];' if f['many'] else f'{f["typ"]}? {f["prop"]} {{ get; init; }}')

def local_decl(f):
    value = 'null'
    if f['attribute']:
        value = f'_r.GetAttribute("{f["name"]}")' if f['typ']=='string' else 'null'
    return f'        {"List<"+f["typ"]+">" if f["many"] else f["typ"]}? extra{f["prop"]} = {value};'

def case_decl(f):
    assignment = f'(extra{f["prop"]} ??= []).Add({f["expr"]})' if f['many'] else f'extra{f["prop"]} = {f["expr"]}'
    return f'                case "{f["name"]}": {assignment}; return true;'

def initializer(f): return f'{f["prop"]} = extra{f["prop"]}'+(' ?? []' if f['many'] else '')

def build_new(typ, method, node, path):
    if method in new_methods: return
    fields = [field_def(f, typ, path+'/'+('@' if f['attribute'] else '')+f['name']) for f in schema.children(node)]
    mono = typ in ('Icms02','Icms15','Icms53','Icms61')
    models[typ] = 'public sealed record '+typ+(' : Icms' if mono else '')+'\n{\n'+'\n\n'.join(prop_decl(f) for f in fields if not mono or f['name']!='orig')+'\n}\n'
    decl = '\n'.join(local_decl(f) for f in fields)
    cases = '\n'.join(case_decl(f) for f in fields)
    init = ',\n            '.join(('Orig = (OrigemMercadoria)int.Parse(extraOrig ?? "0", CultureInfo.InvariantCulture)' if mono and f['name']=='orig' else initializer(f)) for f in fields)
    new_methods[method] = f'''    private async Task<{typ}> {method}(CancellationToken ct)
    {{
{decl}
        await _r.ConsumeChildrenAsync(async nome =>
        {{
            switch (nome)
            {{
{cases}
                default: return false;
            }}
        }}, _options.Strict, ct).ConfigureAwait(false);
        return new {typ}
        {{
            {init}
        }};
    }}
'''
    for f in fields: bindings.append(dict(path=f['path'], model=typ, property=f['prop'], reader=method))

def extend(method, node, path):
    if (method,path) in visited: return
    visited.add((method,path))
    entry = methods[method]
    text = entry['text']
    cases = {m[1]:m[2] for m in re.finditer(r'case "([^"]+)":(.*?)(?=\n\s*case |\n\s*default:)', text, re.S)}
    missing = []
    for field in schema.children(node):
        name = field['name']
        p = path+'/'+('@' if field['attribute'] else '')+name
        child_type = schema.type_node(field['node'])
        if name in cases:
            child_method = re.search(r'(Read\w+Async)\(ct\)', cases[name])
            if child_method and child_method[1] in methods and child_type is not None:
                extend(child_method[1], child_type, p)
            bindings.append(dict(path=p, model=entry['type'], property=pascal(name), reader=method))
        elif field['attribute'] and f'GetAttribute("{name}")' in text:
            bindings.append(dict(path=p, model=entry['type'], property=pascal(name), reader=method))
        else:
            f = field_def(field, entry['type'], p)
            if method == 'ReadIcmsAsync':
                text = text.replace('                default: return false;', f'                case "{name}": icms = {f["expr"]}; return true;\n                default: return false;')
                continue
            missing.append(f)
            bindings.append(dict(path=p, model=entry['type'], property=f['prop'], reader=method))
    if missing:
        additions.setdefault(entry['type'], {}).update({f['name']:f for f in missing})
        text = text.replace('        await _r.ConsumeChildrenAsync', '\n'.join(local_decl(f) for f in missing)+'\n\n        await _r.ConsumeChildrenAsync', 1)
        text = text.replace('                default:', '\n'.join(case_decl(f) for f in missing)+'\n                default:', 1)
        init = ', '.join(initializer(f) for f in missing)
        if entry['type']=='InfNFe':
            text = text.replace('            itens);', '            itens) { '+init+' };')
        else:
            text = re.sub(r'(return new '+entry['type']+r'\s*\{)', lambda m:m[0]+' '+init+', ', text, count=1)
        entry['text'] = text
    else: entry['text'] = text

inf = schema.children(schema.types['TNFe'])[0]['node']
extend('ReadInfNFeAsync', schema.type_node(inf), 'TNFe/infNFe')
# Supplemental information is integrated by routing in task 6.
supl = next(f for f in schema.children(schema.types['TNFe']) if f['name']=='infNFeSupl')
build_new('InformacaoSuplementar', 'ReadInformacaoSuplementarAsync', schema.type_node(supl['node']), 'TNFe/infNFeSupl')
for method, entry in methods.items():
    files[entry['file']] = files[entry['file']].replace(entry['old'], entry['text'])
for p, text in files.items(): p.write_text(text, encoding='utf-8')

header = '// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.\n'
new_dir = SRC/'Modelos/Complementos'
new_dir.mkdir(exist_ok=True)
for typ, body in models.items():
    (new_dir/(typ+'.cs')).write_text(header+'using TecnoFisc.Sped.Core.Enums;\n\nnamespace TecnoFisc.Sped.NFeNFCe;\n\n'+body, encoding='utf-8')
for typ, fs in additions.items():
    if typ=='InfNFe': continue
    p = SRC/'Modelos'/(typ+'.cs')
    text = p.read_text(encoding='utf-8-sig').replace('sealed record '+typ, 'sealed partial record '+typ)
    p.write_text(text, encoding='utf-8')
    (new_dir/(typ+'.cs')).write_text(header+'namespace TecnoFisc.Sped.NFeNFCe;\n\npublic sealed partial record '+typ+'\n{\n'+'\n\n'.join(prop_decl(f) for f in fs.values())+'\n}\n', encoding='utf-8')
# Internal infNFe builder and public note share additions without another object graph.
fs = additions.get('InfNFe', {})
p = SRC/'Parser/NFeXmlReader.cs'
text = p.read_text(encoding='utf-8')
text = text.replace('Total Total, IReadOnlyList<Item> Itens);', 'Total Total, IReadOnlyList<Item> Itens)\n    {\n'+'\n'.join(prop_decl(f) for f in fs.values())+'\n    }')
text = text.replace('            Protocolo = protocolo,', '            Protocolo = protocolo,\n'+ '\n'.join(f'            {f["prop"]} = core.{f["prop"]},' for f in fs.values()))
p.write_text(text, encoding='utf-8')
p = SRC/'Modelos/NFe.cs'
p.write_text(p.read_text(encoding='utf-8-sig').replace('sealed record NFe', 'sealed partial record NFe'), encoding='utf-8')
(new_dir/'NFe.cs').write_text(header+'namespace TecnoFisc.Sped.NFeNFCe;\n\npublic sealed partial record NFe\n{\n'+'\n\n'.join(prop_decl(f) for f in fs.values())+'\n}\n', encoding='utf-8')
# Split helpers into individual files; compiler sees ordinary direct switch readers.
reader_dir = SRC/'Parser/Complementos'
reader_dir.mkdir(exist_ok=True)
for method, body in new_methods.items():
    (reader_dir/(method+'.cs')).write_text(header+'using System.Globalization;\nusing System.Xml;\nusing TecnoFisc.Sped.Core.Enums;\n\nnamespace TecnoFisc.Sped.NFeNFCe.Parser;\n\ninternal sealed partial class NFeXmlReader\n{\n'+body+'}\n', encoding='utf-8')
(ROOT/'sped/nfe/field-bindings.json').write_text(json.dumps(bindings, indent=2)+'\n', encoding='utf-8')
print(f'{len(models)} new records; {sum(len(x) for x in additions.values())} added fields; {len(bindings)} bindings')

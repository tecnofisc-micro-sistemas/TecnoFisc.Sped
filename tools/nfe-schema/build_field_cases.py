"""Build independent lexical expectations from the pinned XSD, not from C# bodies."""
import json
from pathlib import Path
import xml.etree.ElementTree as ET
from inventory import Schema, XS

ROOT = Path(__file__).resolve().parents[2]
schema = Schema(ROOT/'sped/nfe/schemas')
bindings = json.loads((ROOT/'sped/nfe/field-bindings.json').read_text())
KEY = '31210711222333000181550050007156531436322400'

def enum_value(node):
    tn = schema.type_node(node)
    if tn is None: return None
    restriction = tn.find(XS+'restriction')
    if restriction is None: return None
    val = restriction.find(XS+'enumeration')
    if val is not None: return val.get('value')
    base = restriction.get('base','').split(':')[-1]
    if base in schema.types: return enum_value(ET.Element(XS+'element', type=base))
    return None

def value(field, path):
    n = field['name']
    fixed = {'Id':'NFe'+KEY,'versao':'4.00','mod':'55','CNPJ':'11222333000181','CNPJProd':'11222333000181',
        'CPF':'52998224725','UF':'SP','cUF':'35','cMun':'3550308','cMunFG':'3550308','CFOP':'5102','NCM':'22030000',
        'CEST':'0302100','cEAN':'SEM GTIN','cEANTrib':'SEM GTIN','chNFe':KEY,'refNFe':KEY,'refNFeSig':KEY,'chNFeProd':KEY,
        'IE':'110042490114','orig':'0','nItem':'1','cEnq':'999'}
    if n in fixed: return fixed[n]
    base = schema.base(field['node'])
    if base=='date': return '2026-10-02'
    if base=='dateTime': return '2026-10-02T12:34:56-03:00'
    if base=='decimal': return '12.3456'
    if base=='boolean': return 'true'
    return enum_value(field['node']) or '1'

nodes = {}
def index(node, path):
    nodes[path] = node
    for f in schema.children(node):
        typ = schema.type_node(f['node'])
        if typ is not None and typ.tag==XS+'complexType': index(typ,path+'/'+f['name'])
index(schema.types['TNFe'],'TNFe')
index(schema.types['TProtNFe'],'TProtNFe')

def build(node, name, path):
    root=ET.Element(name)
    for f in schema.children(node):
        if f['name']=='Signature': continue
        if f['attribute']: root.set(f['name'], value(f,path)); continue
        typ=schema.type_node(f['node'])
        if typ is not None and typ.tag==XS+'complexType': root.append(build(typ,f['name'],path+'/'+f['name']))
        else: ET.SubElement(root,f['name']).text=value(f,path)
    return root

cases=[]
for b in bindings:
    if b['path']=='TProtNFe/@versao': continue # Public envelope test covers this attribute.
    parent, leaf = b['path'].rsplit('/',1)
    node=nodes.get(parent)
    if node is None: continue
    f=next((f for f in schema.children(node) if f['name']==leaf.removeprefix('@')),None)
    if f is None or schema.base(f['node'])=='complex': continue
    # The root is handled by the envelope entry point; test its private infNFe reader.
    xml=ET.tostring(build(node,parent.rsplit('/',1)[-1],parent),encoding='unicode')
    prop = 'Chave' if b['model']=='InfNFe' and leaf=='@Id' else b['property']
    expected=value(f,parent)
    if prop=='Chave': expected=KEY
    if leaf=='CST' and '/ICMS/' in parent and b['model'] not in ('Icms02','Icms15','Icms53','Icms61'): expected='0'+expected
    cases.append(dict(path=b['path'], reader=b['reader'], property=prop, xml=xml, expected=expected, base=schema.base(f['node'])))
target=ROOT/'tests/TecnoFisc.Sped.NFeNFCe.Tests/Fixtures/schema-fields.json'
target.write_text(json.dumps(cases,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(len(cases),'leaf expectations')

repeated=[]
by_path={b['path']:b for b in bindings}
for row in schema.inventory() + schema.inventory('TProtNFe'):
    if row['max']==1: continue
    path=row['path']; parent,leaf=path.rsplit('/',1)
    b=by_path[path]
    root=build(nodes[parent],parent.rsplit('/',1)[-1],parent)
    child=root.find(leaf)
    root.append(ET.fromstring(ET.tostring(child)))
    repeated.append(dict(path=path,reader=b['reader'],property=b['property'],xml=ET.tostring(root,encoding='unicode')))
target.with_name('schema-repetitions.json').write_text(json.dumps(repeated,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(len(repeated),'repetition expectations')

groups=[]
seen=set()
for row in schema.inventory():
    path=row['path']
    if row['base']!='complex' or path.count('/')==1 or path in seen: continue
    seen.add(path)
    parent,leaf=path.rsplit('/',1); b=by_path[path]
    root=build(nodes[parent],parent.rsplit('/',1)[-1],parent)
    if b['property']=='$variant' or b['property']=='Tributacao':
        for child in list(root):
            if child.tag!=leaf and child.tag in ('IPITrib','IPINT') or b['property']=='$variant' and child.tag!=leaf:
                root.remove(child)
    child_binding=next(x for x in bindings if x['path'].rsplit('/',1)[0]==path)
    groups.append(dict(path=path,reader=b['reader'],property=b['property'],model=child_binding['model'],xml=ET.tostring(root,encoding='unicode')))
target.with_name('schema-groups.json').write_text(json.dumps(groups,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(len(groups),'group expectations')

import tempfile
import unittest
from pathlib import Path
from inventory import Schema


class InventoryTests(unittest.TestCase):
    def schema(self, body):
        tmp = tempfile.TemporaryDirectory()
        self.addCleanup(tmp.cleanup)
        path = Path(tmp.name)
        (path / 'test.xsd').write_text('<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">' + body + '</xs:schema>')
        return Schema(path)

    def test_shared_types_choices_repetition_attributes(self):
        schema = self.schema('''
        <xs:complexType name="Shared"><xs:sequence><xs:element name="value" type="xs:decimal"/></xs:sequence><xs:attribute name="id" type="xs:string"/></xs:complexType>
        <xs:complexType name="Root"><xs:sequence><xs:element name="first" type="Shared"/><xs:choice minOccurs="0"><xs:element name="second" type="Shared" maxOccurs="10"/><xs:element name="other" type="xs:string"/></xs:choice></xs:sequence></xs:complexType>''')
        rows = {x['path']: x for x in schema.inventory('Root')}
        self.assertIn('Root/first/value', rows)
        self.assertIn('Root/second/@id', rows)
        self.assertEqual(0, rows['Root/second']['min'])
        self.assertEqual(10, rows['Root/second']['max'])
        self.assertTrue(rows['Root/second']['choice'])
        self.assertEqual('decimal', rows['Root/first/value']['base'])

    def test_cycle_is_rejected(self):
        schema = self.schema('<xs:complexType name="Loop"><xs:sequence><xs:element name="self" type="Loop"/></xs:sequence></xs:complexType>')
        with self.assertRaisesRegex(ValueError, 'Cycle'):
            schema.inventory('Loop')

    def test_new_shared_leaf_changes_both_paths(self):
        schema = self.schema('<xs:complexType name="Shared"><xs:sequence><xs:element name="new" type="xs:string"/></xs:sequence></xs:complexType><xs:complexType name="Root"><xs:sequence><xs:element name="a" type="Shared"/><xs:element name="b" type="Shared"/></xs:sequence></xs:complexType>')
        paths = {x['path'] for x in schema.inventory('Root')}
        self.assertTrue({'Root/a/new', 'Root/b/new'}.issubset(paths))

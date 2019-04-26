using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.MapperService
{
    public class Mapper
    {
        public static void Map(Object src, 
                               Object dst, 
                               Mappings mappings = null, 
                               bool nonNullPropertiesOnly = false) {

            var srcT = src.GetType();
            var dstT = dst.GetType();

            foreach (var f in srcT.GetFields()) {
                if (f.FieldType.IsValueType || f.FieldType == typeof(string)) {
                    string targetField = GetTargetFieldName(f.Name, mappings);
                    if (targetField != null) {
                        var dstF = dstT.GetField(targetField);
                        if (dstF != null) {
                            var value = f.GetValue(src);
                            if (nonNullPropertiesOnly) {
                                if (value != null) dstF.SetValue(dst, value);
                            } else {
                                dstF.SetValue(dst, value);
                            }
                        }
                    }
                }
            }

            foreach (var f in srcT.GetProperties()) {
                if (f.PropertyType.IsValueType || f.PropertyType == typeof(string)) {
                    string targetField = GetTargetFieldName(f.Name, mappings);
                    if (targetField != null) {
                        var dstF = dstT.GetProperty(targetField);
                        if (dstF != null) {
                            var value = f.GetValue(src);
                            if (nonNullPropertiesOnly) {
                                if (value != null) dstF.SetValue(dst, value);
                            } else {
                                if(dstF.CanWrite) dstF.SetValue(dst, value);
                            }
                        }
                    }
                }
            }
        }

        static string GetTargetFieldName(string sourceFieldName, Mappings mappings) {
            string rc = sourceFieldName;
            if(mappings != null) {
                foreach(var mapping in mappings.FieldMappings) {
                    if(mapping.SourceField == sourceFieldName) {
                        if (string.IsNullOrEmpty(mapping.TargetField) || mapping.TargetField == sourceFieldName) {
                            // Mapping null, so don't copy
                            rc = null;
                            break;

                        } else {
                            // Return the mapping
                            rc = mapping.TargetField;
                            break;
                        }
                    }
                }
            }
            return rc;
        }
    }

    public class MapperFieldMapping {
        public MapperFieldMapping(string sourceField, string targetField) {
            SourceField = sourceField;
            TargetField = targetField;
        }
        public string SourceField { set; get; }
        public string TargetField { set; get; }
    }

    public class Mappings {
        public List<MapperFieldMapping> FieldMappings { set; get; } = new List<MapperFieldMapping>();

        public void Clear() {
            FieldMappings = new List<MapperFieldMapping>();
        }

        public void Add(string sourceField, string targetField) {
            if (sourceField != targetField) {
                // Fields cannot map to themselves
                FieldMappings.Add(new MapperFieldMapping(sourceField, targetField));

                if (!string.IsNullOrEmpty(targetField)) {
                    // The target field is having data redirected to it, so disable its
                    // own mapping so it isn't copied to itself as this could overwrite the
                    // redirection.
                    FieldMappings.Add(new MapperFieldMapping(targetField, null));
                }
            }
        }
    }
}

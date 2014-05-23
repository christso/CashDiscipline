using System;
using System.Collections.Generic;
using System.Linq;

namespace CTMS.Module.HelperClasses.Xpo
{
    public class FieldMaps
    {
        public FieldMaps()
        {
            _sourceFieldMaps = new Dictionary<string, FieldMap>();
            _targetFieldMaps = new Dictionary<string, FieldMap>();
        }

        private Dictionary<string, FieldMap> _sourceFieldMaps;
        private Dictionary<string, FieldMap> _targetFieldMaps;

        public Dictionary<string, FieldMap> Source
        {
            get { return _sourceFieldMaps; }
        }

        public Dictionary<string, FieldMap> Target
        {
            get { return _targetFieldMaps; }
        }

        public void Add(string sourceField, string targetField)
        {
            var obj = new FieldMap() { SourceField = sourceField, 
                TargetField = targetField };
            _sourceFieldMaps.Add(sourceField, obj);
            _targetFieldMaps.Add(targetField, obj);
        }

        public void Add(FieldMap map)
        {
            _sourceFieldMaps.Add(map.SourceField, map);
            _targetFieldMaps.Add(map.TargetField, map);
        }
    }

    public class FieldMap
    {
        public string SourceField;
        public int SourceOrdinal;
        public SourceValueDelegate SourceValue;
        public string TargetField;
    }
    public delegate object SourceValueDelegate(string value);
}

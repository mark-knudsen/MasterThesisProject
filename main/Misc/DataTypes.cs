namespace MyCompiler
{
    public abstract class Type
    {
        public abstract override string ToString();
    }
    public class IntType : Type
    {
        public override string ToString() { return "int"; }
    }
    public class FloatType : Type
    {
        public override string ToString() { return "float"; }
    }
    public class BoolType : Type
    {
        public override string ToString() { return "bool"; }
    }
    public class StringType : Type
    {
        public override string ToString() { return "string"; }
    }
    public class VoidType : Type
    {
        public override string ToString() { return "void"; }
    }

    public class ArrayType : Type
    {
        public Type ElementType { get; }

        public ArrayType(Type elementType)
        {
            ElementType = elementType;
        }

        public override string ToString()
        {
            return "array" + "(" + ElementType.ToString() + ")";
        }
    }

    public class RecordType : Type
    {
        public List<RecordField> RecordFields { get; }

        public RecordType(List<RecordField> recordFields)
        {
            RecordFields = recordFields;
        }

        public override string ToString()
        {
            string returnVal = "record(";
            foreach (var item in RecordFields)
            {
                returnVal += item.Label + ", ";
            }
            returnVal = returnVal[..^2];
            return returnVal + ")";
        }
    }

    public class DataframeType : Type
    {
        public List<string> Columns { get; }
        public List<List<object>> DataPointers { get; }
        public List<Type> DataTypes { get; }

        public DataframeType(List<string> columns, List<List<object>> dataPointers, List<Type> dataTypes)
        {
            Columns = columns;
            DataPointers = dataPointers;
            DataTypes = dataTypes;
        }

        public override string ToString()
        {
            string returnVal = "dataframe(";
            for (int i = 0; i < Columns.Count; i++)
            {
                returnVal += Columns[i] + ": " + DataTypes[i].ToString() + ", ";
            }
            returnVal = returnVal[..^2];
            return returnVal + ")";
        }
    }
}
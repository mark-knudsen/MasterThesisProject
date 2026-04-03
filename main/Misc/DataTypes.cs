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
    public class NullType : Type
    {
        public override string ToString() { return "null"; }
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
        public List<string> ColumnNames { get; }
        public List<Type> DataTypes { get; } // NEW: Store the type for each column
        public RecordType RowType { get; }     // The full record structure

        public DataframeType(List<string> names, List<Type> types, RecordType rowType)
        {
            ColumnNames = names;
            DataTypes = types;
            RowType = rowType;
        }

        public override string ToString() =>
            $"dataframe({string.Join(", ", ColumnNames.Select((n, i) => $"{n}: {DataTypes[i]}"))})";
    }


}
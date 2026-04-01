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
        public List<string> Columns { get; }
        public RecordType RowType { get; }

        public DataframeType(List<string> columns, RecordType rowType)
        {
            Columns = columns;
            RowType = rowType;
        }

        public RecordType AsRecordType()
        {
            return new RecordType(new List<RecordField>
            {
                new RecordField
                {
                    Label = "columns",
                    Type = new ArrayType(new StringType()) // pseudo value holder
                },
                new RecordField
                {
                    Label = "rows",
                    Type = new ArrayType(RowType)
                }
            });
        }

        public override string ToString()
        {
            string returnVal = "dataframe(";
            foreach (var item in Columns)
            {
                returnVal += item + ", ";
            }
            returnVal = returnVal[..^2];
            return returnVal + ")";
        }
    }

}
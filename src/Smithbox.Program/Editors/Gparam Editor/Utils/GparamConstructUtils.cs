using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public static class GparamConstructUtils
{
    public static Param AddNewParam(ProjectEntry project, GparamAnnotationEntry annotation)
    {
        var newParam = new Param();

        newParam.Key = annotation.ID;
        newParam.Name = annotation.Name;
        newParam.Fields = new();

        if(project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or 
            ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            newParam.Comments = new();
        }

        // Add the fields (derived from the stored Type in the annotation)
        foreach(var field in annotation.Fields)
        {
            switch(field.Type)
            {
                case "SbyteField":
                    newParam.Fields.Add(AddNewSbyteField(field));
                    break;

                case "ShortField":
                    newParam.Fields.Add(AddNewShortField(field));
                    break;

                case "IntField":
                    newParam.Fields.Add(AddNewIntField(field));
                    break;

                case "ByteField":
                    newParam.Fields.Add(AddNewByteField(field));
                    break;

                case "UintField":
                    newParam.Fields.Add(AddNewUintField(field));
                    break;

                case "FloatField":
                    newParam.Fields.Add(AddNewFloatField(field));
                    break;

                case "BoolField":
                    newParam.Fields.Add(AddNewBoolField(field));
                    break;

                case "Vector2Field":
                    newParam.Fields.Add(AddNewVector2Field(field));
                    break;

                case "Vector3Field":
                    newParam.Fields.Add(AddNewVector3Field(field));
                    break;

                case "Vector4Field":
                    newParam.Fields.Add(AddNewVector4Field(field));
                    break;

                case "ColorField":
                    newParam.Fields.Add(AddNewColorField(field));
                    break;

                case "LongField":
                    newParam.Fields.Add(AddNewLongField(field));
                    break;

                case "UshortField":
                    newParam.Fields.Add(AddNewUshortField(field));
                    break;

                case "UlongField":
                    newParam.Fields.Add(AddNewUlongField(field));
                    break;

                case "DoubleField":
                    newParam.Fields.Add(AddNewDoubleField(field));
                    break;

                case "StringField":
                    newParam.Fields.Add(AddNewStringField(field));
                    break;

                default: throw new Exception("Unknown gparam field type");
            }
        }

        return newParam;
    }

    public static IField AddNewField(GparamAnnotationFieldEntry fieldEntry)
    {
        switch (fieldEntry.Type)
        {
            case "SbyteField":
                return AddNewSbyteField(fieldEntry);

            case "ShortField":
                return AddNewShortField(fieldEntry);

            case "IntField":
                return AddNewIntField(fieldEntry);

            case "ByteField":
                return AddNewByteField(fieldEntry);

            case "UintField":
                return AddNewUintField(fieldEntry);

            case "FloatField":
                return AddNewFloatField(fieldEntry);

            case "BoolField":
                return AddNewBoolField(fieldEntry);

            case "Vector2Field":
                return AddNewVector2Field(fieldEntry);

            case "Vector3Field":
                return AddNewVector3Field(fieldEntry);

            case "Vector4Field":
                return AddNewVector4Field(fieldEntry);

            case "ColorField":
                return AddNewColorField(fieldEntry);

            case "LongField":
                return AddNewLongField(fieldEntry);

            case "UshortField":
                return AddNewUshortField(fieldEntry);

            case "UlongField":
                return AddNewUlongField(fieldEntry);

            case "DoubleField":
                return AddNewDoubleField(fieldEntry);

            case "StringField":
                return AddNewStringField(fieldEntry);

            default: throw new Exception("Unknown gparam field type");
        }
    }

    public static SbyteField AddNewSbyteField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new SbyteField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<sbyte>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static ShortField AddNewShortField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new ShortField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<short>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static IntField AddNewIntField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new IntField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<int>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static ByteField AddNewByteField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new ByteField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<byte>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static UintField AddNewUintField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new UintField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<uint>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static FloatField AddNewFloatField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new FloatField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<float>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static BoolField AddNewBoolField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new BoolField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<bool>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = false;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static Vector2Field AddNewVector2Field(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new Vector2Field();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<Vector2>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = new Vector2();

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static Vector3Field AddNewVector3Field(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new Vector3Field();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<Vector3>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = new Vector3();

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static Vector4Field AddNewVector4Field(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new Vector4Field();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<Vector4>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = new Vector4();

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static ColorField AddNewColorField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new ColorField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<Color>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = new Color();

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static LongField AddNewLongField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new LongField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<long>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static UshortField AddNewUshortField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new UshortField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<ushort>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static UlongField AddNewUlongField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new UlongField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<ulong>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static DoubleField AddNewDoubleField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new DoubleField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<double>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = 0;

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }

    public static StringField AddNewStringField(GparamAnnotationFieldEntry fieldEntry)
    {
        var field = new StringField();

        field.Key = fieldEntry.ID;
        field.Name = fieldEntry.Name;
        field.Unk = 0;
        field.Capacity = 1;

        var defaultValue = new FieldValue<string>();
        defaultValue.ID = 0;
        defaultValue.TimeOfDay = 0;
        defaultValue.Value = "";

        field.Values = new()
        {
            defaultValue
        };

        return field;
    }
}

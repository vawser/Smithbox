using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using static SoulsFormats.TAE;

namespace SoulsFormats
{
    public partial class TAE : SoulsFile<TAE>
    {
        /// <summary>
        /// An action or effect triggered at a certain time during an animation.
        /// </summary>
        public class Event
        {
            /// <summary>
            /// A parameter in an event.
            /// </summary>
            public class ParameterContainer
            {
                public Dictionary<string, object> ParameterValues;

                /// <summary>
                /// The template of the event for which these are the parameters.
                /// </summary>
                public Template.EventTemplate Template { get; private set; }

                /// <summary>
                /// Returns all parameters.
                /// </summary>
                public Dictionary<string, object> Values
                    => ParameterValues;

                /// <summary>
                /// Value of the specified parameter.
                /// </summary>
                public object this[string paramName]
                {
                    get => ParameterValues[paramName];
                    set => ParameterValues[paramName] = value;
                }

                /// <summary>
                /// Gets the value of a parameter.
                /// </summary>
                public object GetParamValue(string paramName)
                {
                    return this[paramName];
                }

                /// <summary>
                /// Gets the value type of a parameter.
                /// </summary>
                public Template.ParamType GetParamValueType(string paramName)
                {
                    return Template[paramName].Type;
                }

                /// <summary>
                /// Gets the whole template of a parameter.
                /// </summary>
                public Template.ParameterTemplate GetParamTemplate(string paramName)
                {
                    return Template[paramName];
                }

                internal ParameterContainer(long animID, int eventIndex,
                    bool bigEndian, byte[] paramData, Template.EventTemplate template, bool suppressAssert = false)
                {
                    ParameterValues = new Dictionary<string, object>();
                    Template = template;
                    using (var memStream = new System.IO.MemoryStream(paramData))
                    {
                        var br = new BinaryReaderEx(bigEndian, memStream.ToArray());
                        int i = 0;
                        foreach (var paramKvp in Template)
                        {
                            var p = paramKvp.Value;
                            if (p.ValueToAssert != null)
                            {
                                if (!suppressAssert)
                                {
                                    try
                                    {
                                        p.AssertValue(br);
                                    }
                                    catch (System.IO.InvalidDataException ex)
                                    {
                                        var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                        var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                        throw new Exception($"Animation {animID}\nEvent[{eventIndex}] (Type: {txtEventType})" +
                                                $"\n  -> Assert failed on field {txtField} (Type: {p.Type})", ex);
                                    }
                                }
                                else
                                {
                                    // lol I'm sorry TK
                                    p.ReadValue(br);
                                }
                            }
                            else
                            {

                                try
                                {
                                    ParameterValues.Add(p.GetKeyString(), p.ReadValue(br));
                                }
                                catch (Exception ex)
                                {
                                    var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                    var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                    throw new Exception($"Animation {animID}\nEvent[{eventIndex}] (Type: {txtEventType})" +
                                            $"\n  -> Failed to read value of field {txtField} (Type: {p.Type})", ex);
                                }
                            }
                            i++;
                        }
                    }
                }

                internal ParameterContainer(bool bigEndian, byte[] paramData, Template.EventTemplate template, bool suppressAssert = false)
                {
                    ParameterValues = new Dictionary<string, object>();
                    Template = template;
                    using (var memStream = new System.IO.MemoryStream(paramData))
                    {
                        var br = new BinaryReaderEx(bigEndian, memStream.ToArray());
                        int i = 0;
                        foreach (var paramKvp in Template)
                        {
                            var p = paramKvp.Value;
                            if (p.ValueToAssert != null)
                            {
                                if (!suppressAssert)
                                {
                                    try
                                    {
                                        p.AssertValue(br);
                                    }
                                    catch (System.IO.InvalidDataException ex)
                                    {
                                        var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                        var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                        throw new Exception($"Event Type: {txtEventType}" +
                                                $"\n  -> Assert failed on field {txtField}", ex);
                                    }
                                }
                                else
                                {
                                    p.ReadValue(br);
                                }
                            }
                            else
                            {
                                try
                                {
                                    ParameterValues.Add(p.GetKeyString(), p.ReadValue(br));
                                }
                                catch (Exception ex)
                                {
                                    var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                    var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                    throw new Exception($"Event Type: {txtEventType}" +
                                            $"\n  -> Failed to read value of field {txtField} (Type: {p.Type})", ex);
                                }
                            }
                            i++;
                        }
                    }
                }

                internal byte[] AsBytes(bool bigEndian)
                {
                    using (var memStream = new System.IO.MemoryStream())
                    {
                        var bw = new BinaryWriterEx(bigEndian, memStream);

                        foreach (var paramKvp in Template)
                        {
                            var p = paramKvp.Value;
                            if (p.ValueToAssert != null)
                            {
                                p.WriteValue(bw, p.ValueToAssert);
                            }
                            else
                            {
                                p.WriteValue(bw, this[p.GetKeyString()]);
                            }

                        }

                        return memStream.ToArray();
                    }
                }
            }

            /// <summary>
            /// The type of this event.
            /// </summary>
            public int Type { get; private set; }

            public void ForceChangeType(int newType)
            {
                Type = newType;
            }

            /// <summary>
            /// TAE Event Group which contains this event.
            /// </summary>
            public TAE.EventGroup Group { get; set; }

            /// <summary>
            /// An unknown 32-bit integer following the event type.
            /// So far confirmed to be used in SOTFS and SDT
            /// </summary>
            public int Unk04 { get; private set; }

            /// <summary>
            /// When the event begins.
            /// </summary>
            public float StartTime;

            /// <summary>
            /// When the event ends.
            /// </summary>
            public float EndTime;

            public const float EldenRingInfiniteLengthEventPlaceholder = 100;

            public float MemeEndTime => EndTime >= EldenRingInfiniteLengthEventPlaceholder ? float.MaxValue : EndTime;

            internal byte[] ParameterBytes;

            /// <summary>
            /// Gets the bytes of this event's parameters. This will
            /// properly return ready-to-save bytes if a template
            /// is being used, otherwise it returns the bytes
            /// read directly from the file (which may
            /// include some padding).
            /// </summary>
            public byte[] GetParameterBytes(bool bigEndian)
            {
                if (Parameters != null)
                    CopyParametersToBytes(bigEndian);
                return ParameterBytes;
            }

            /// <summary>
            /// Sets this event's parameter bytes to those specified. Updates the
            /// .Parameters template values as well if a template is applied.
            /// </summary>
            public void SetParameterBytes(bool bigEndian, byte[] parameterBytes, bool lenientOnAssert = false)
            {
                if (parameterBytes.Length != ParameterBytes.Length)
                    throw new ArgumentException("Not the same amount of bytes as was originally here.");

                ParameterBytes = parameterBytes;

                if (Parameters != null)
                {
                    var prevTemplate = Template;
                    Parameters = null;
                    ApplyTemplate(bigEndian, prevTemplate, lenientOnAssert);
                }
            }

            public bool IsIdenticalTo(Event otherEvent, bool bigEndian)
            {
                var paramBytes = GetParameterBytes(bigEndian);
                var otherParamBytes = otherEvent.GetParameterBytes(bigEndian);
                return Type == otherEvent.Type && paramBytes.SequenceEqual(otherParamBytes);
            }

            public Event GetClone(bool bigEndian)
            {
                var paramBytes = GetParameterBytes(bigEndian);
                var ev = new Event(StartTime, EndTime, Type, Unk04, paramBytes, bigEndian);
                if (Parameters != null)
                    ev.Parameters = new ParameterContainer(bigEndian, paramBytes, Parameters.Template);
                return ev;
            }

            /// <summary>
            /// Indexable parameter container of this event.
            /// Use .Parameters[name] for basic value get/set
            /// and use .GetValueType(name) to see how to convert
            /// it to/from System.Object.
            /// </summary>
            public ParameterContainer Parameters { get; private set; }

            /// <summary>
            /// The EventTemplate applied to this event, if any.
            /// </summary>
            public Template.EventTemplate Template
                => Parameters?.Template ?? null;

            /// <summary>
            /// Gets the name of this event's type if a template has been loaded.
            /// Otherwise returns null.
            /// </summary>
            public string TypeName
                => Parameters?.Template?.Name;

            /// <summary>
            /// Applies a template to allow editing of the parameters.
            /// </summary>
            internal void ApplyTemplate(TAE containingTae, Template template,
                long animID, int eventIndex, int eventType)
            {

                if (template.Events.ContainsKey(eventType))
                {
                    if (Parameters != null)
                    {
                        CopyParametersToBytes(containingTae.BigEndian);
                    }
                    Type = eventType;
                    Array.Resize(ref ParameterBytes, template.Events[Type].GetAllParametersByteCount());
                    Parameters = new ParameterContainer(animID, eventIndex,
                        containingTae.BigEndian, ParameterBytes, template.Events[Type]);

                }
            }

            internal void ChangeTemplateAfterLoading(TAE containingTae, Template template,
                long animID, int eventIndex, int eventType)
            {
                if (template.Events.ContainsKey(eventType))
                {
                    Type = eventType;
                    Array.Resize(ref ParameterBytes, template.Events[Type].GetAllParametersByteCount());

                    var newParameters = new ParameterContainer(animID, eventIndex,
                        containingTae.BigEndian, ParameterBytes, template.Events[Type],
                        suppressAssert: true);
                    if (Parameters != null)
                    {
                        foreach (var field in template.Events[Type])
                        {
                            if (field.Value.ValueToAssert != null)
                                continue;
                            if (Parameters.Template.ContainsKey(field.Key))
                                newParameters[field.Key] = Parameters[field.Key];
                        }
                    }

                }
            }

            /// <summary>
            /// Applies a template to allow editing of the parameters.
            /// </summary>
            public void ApplyTemplate(bool isBigEndian, Template.EventTemplate template, bool lenientOnAssert = false)
            {
                if (template.ID != Type)
                {
                    throw new ArgumentException($"Template is for event type {template.ID} but this event is type {Type}");
                }
                if (Parameters != null)
                {
                    CopyParametersToBytes(isBigEndian);
                }
                Array.Resize(ref ParameterBytes, template.GetAllParametersByteCount());
                Parameters = new ParameterContainer(isBigEndian, ParameterBytes, template, suppressAssert: lenientOnAssert);
            }

            private void CopyParametersToBytes(bool isBigEndian)
            {
                if (Parameters != null)
                    ParameterBytes = Parameters.AsBytes(isBigEndian);
            }

            /// <summary>
            /// Applies a template to this TAE for editing and also wipes all
            /// values and replaces them with default values.
            /// </summary>
            public void ApplyTemplateWithDefaultValues(bool isBigEndian, Template.EventTemplate template)
            {
                Type = template.ID;
                ParameterBytes = template.GetDefaultBytes(isBigEndian);
                Parameters = new ParameterContainer(isBigEndian, ParameterBytes, template);
            }

            /// <summary>
            /// Creates a new event with the specified start time, end time, type, and unknown then
            /// applies default values from the provided template.
            /// </summary>
            public Event(float startTime, float endTime, int type, int unk04, bool isBigEndian, Template.EventTemplate template)
            {
                StartTime = startTime;
                EndTime = endTime;
                Type = type;
                Unk04 = unk04;
                ApplyTemplateWithDefaultValues(isBigEndian, template);
            }

            /// <summary>
            /// Creates a new event with the specified start time, end time, type, unknown, and parameters.
            /// </summary>
            public Event(float startTime, float endTime, int type, int unk04, byte[] parameters, bool isBigEndianParameters)
            {
                StartTime = startTime;
                EndTime = endTime;
                Type = type;
                Unk04 = unk04;
                ParameterBytes = parameters;
                if (Template != null)
                {
                    Parameters = new ParameterContainer(isBigEndianParameters, parameters, Template);
                }
            }

            internal Event(float startTime, float endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }

            internal void WriteHeader(BinaryWriterEx bw, int animIndex, int eventIndex, Dictionary<float, long> timeOffsets, TAEFormat format)
            {
                bw.WriteVarint(timeOffsets[StartTime]);
                bw.WriteVarint(timeOffsets[MemeEndTime]);
                bw.ReserveVarint($"EventDataOffset{animIndex}:{eventIndex}");
                if (format is TAEFormat.DES or TAEFormat.DESR) // Not sure if in DESR
                    bw.Pad(0x10);
            }

            internal void WriteData(BinaryWriterEx bw, int animIndex, int eventIndex, TAEFormat format)
            {
                CopyParametersToBytes(bw.BigEndian);

                bw.FillVarint($"EventDataOffset{animIndex}:{eventIndex}", bw.Position);
                bw.WriteInt32(Type);

                if (format is not TAEFormat.DS1)
                    bw.WriteInt32(Unk04);

                bool isWeirdEventWithNoParamOffset = format == TAEFormat.SDT && Type == 943;

                if (isWeirdEventWithNoParamOffset)
                    bw.WriteVarint(0);
                else
                    bw.ReserveVarint($"EventDataParametersOffset{animIndex}:{eventIndex}");

                if (format is TAEFormat.DES) // Not in DESR
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    //bw.Pad(0x10);
                }

                if (!isWeirdEventWithNoParamOffset)
                    bw.FillVarint($"EventDataParametersOffset{animIndex}:{eventIndex}", bw.Position);

                bw.WriteBytes(ParameterBytes);

                if (bw.VarintLong || format is TAEFormat.DES)
                    bw.Pad(0x10);
            }

            internal void ReadParameters(BinaryReaderEx br, int byteCount)
            {
                ParameterBytes = br.ReadBytes(byteCount);
            }

            /// <summary>
            /// Returns the start time, end time, and type of the event.
            /// </summary>
            public override string ToString()
            {
                return $"Start Frame: {(int)Math.Round(StartTime * 30):D3} - End Frame: {(int)Math.Round(EndTime * 30):D3} Event Type: {Type}";
            }

            internal static Event Read(BinaryReaderEx br, out long parametersOffset, TAEFormat format)
            {
                long startTimeOffset = br.ReadVarint();
                long endTimeOffset = br.ReadVarint();
                long eventDataOffset = br.ReadVarint();

                if (format is TAEFormat.DES or TAEFormat.DESR)
                    br.Pad(0x10);

                float startTime = br.GetSingle(startTimeOffset);
                float endTime = br.GetSingle(endTimeOffset);

                if (endTime == float.MaxValue)
                    endTime = EldenRingInfiniteLengthEventPlaceholder;

                Event result = new Event(startTime, endTime);
                br.StepIn(eventDataOffset);
                {
                    result.Type = br.ReadInt32();

                    if (br.VarintLong)
                        result.Unk04 = br.ReadInt32();

                    //if (format == TAEFormat.SDT)
                    //{
                    //    // offset will be 0 in sekiro if no parameters
                    //    br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4), 0);
                    //    parametersOffset = br.Position;
                    //}
                    //else
                    //{
                    //    parametersOffset = br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4));
                    //}

                    // Parameters will ALWAYS be right here otherwise it crashes iirc.
                    parametersOffset = br.GetNextPaddedOffsetAfterCurrentField(br.VarintSize, format is TAEFormat.DES ? 0x10 : 0);
                    br.AssertVarint(parametersOffset, 0);
                    br.Position = parametersOffset;
                }
                br.StepOut();

                return result;
            }
        }

    }
}

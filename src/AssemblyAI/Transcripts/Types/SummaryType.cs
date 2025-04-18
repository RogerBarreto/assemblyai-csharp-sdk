using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using AssemblyAI.Core;

#nullable enable

namespace AssemblyAI.Transcripts;

[JsonConverter(typeof(EnumSerializer<SummaryType>))]
public enum SummaryType
{
    [EnumMember(Value = "bullets")]
    Bullets,

    [EnumMember(Value = "bullets_verbose")]
    BulletsVerbose,

    [EnumMember(Value = "gist")]
    Gist,

    [EnumMember(Value = "headline")]
    Headline,

    [EnumMember(Value = "paragraph")]
    Paragraph,
}

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using LMSDataExtraction.Application.Json;

namespace LMSDataExtraction.Application.Dtos.Competences;

// HBO-i competence matrix: 6 layers.
// Wire values must match the chatbot frontend exactly.
[JsonConverter(typeof(EnumMemberJsonConverter<HboiLayer>))]
public enum HboiLayer
{
    [EnumMember(Value = "Professional development")]
    ProfessionalDevelopment,

    [EnumMember(Value = "User Interaction")]
    UserInteraction,

    [EnumMember(Value = "Software")]
    Software,

    [EnumMember(Value = "Hardware Interfacing")]
    HardwareInterfacing,

    [EnumMember(Value = "Infrastructure")]
    Infrastructure,

    [EnumMember(Value = "Organisational processes")]
    OrganisationalProcesses,
}

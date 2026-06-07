using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using LMSDataExtraction.Application.Json;

namespace LMSDataExtraction.Application.Dtos.Competences;

// HBO-i activities:
//  - 5 main activities (Analysis, Advise, Design, Realisation, Manage&Control)
//    valid for the 5 architecture layers (UserInteraction through OrganisationalProcesses).
//  - 2 professional-development activities (Personal leadership, Professional standard)
//    valid for the Professional Development layer.
[JsonConverter(typeof(EnumMemberJsonConverter<HboiActivity>))]
public enum HboiActivity
{
    [EnumMember(Value = "Analysis")]
    Analysis,

    [EnumMember(Value = "Advise")]
    Advise,

    [EnumMember(Value = "Design")]
    Design,

    [EnumMember(Value = "Realisation")]
    Realisation,

    [EnumMember(Value = "Manage&Control")]
    ManageAndControl,

    [EnumMember(Value = "Personal leadership")]
    PersonalLeadership,

    [EnumMember(Value = "Professional standard")]
    ProfessionalStandard,
}

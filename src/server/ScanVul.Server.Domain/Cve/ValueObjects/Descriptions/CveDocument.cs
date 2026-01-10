using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;

public class CveDescriptionDocument
{
    public Payload Payload { get; set; } = null!;
}

public class Payload
{
    public required CveMetadata CveMetadata { get; set; }
    public required Containers Containers { get; set; }
}

public class CveMetadata
{
    public required string CveId { get; set; }
}

public class Containers
{
    [UsedImplicitly]
    public Container? Cna { get; set; }
    
    [UsedImplicitly]
    public List<Container> Adp { get; set; } = [];
}

public class Container
{
    [UsedImplicitly]
    public List<Metrics> Metrics { get; set; } = [];
    
    [UsedImplicitly]
    public List<Description> Descriptions { get; set; } = [];
}

public class Metrics
{
    [DataMember(Name = "cvssV3_1")]
    public Cvss? CvssV31 { get; set; }
    
    [DataMember(Name = "cvssV3_0")]
    public Cvss? CvssV30 { get; set; }
    
    [DataMember(Name = "cvssV2_0")]
    public Cvss? CvssV20 { get; set; }
}

public class Cvss
{
    public required double BaseScore { get; set; }
}

public class Description
{
    public required string Lang { get; set; }
    public required string Value { get; set; }
}
using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class SecureBootTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    internal SecureBootTemplate(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}

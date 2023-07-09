using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenDCSLauncher.Models;

public class BranchInfo
{
    public string? Name { get; set; }
    public string? DirectoryPath { get; set; }

    [IgnoreDataMember]
    public string BinPath => $"{DirectoryPath}\\bin";

    [IgnoreDataMember]
    public string BinMtPath => $"{DirectoryPath}\\bin-mt";

    public override string ToString() => Name ?? string.Empty;
}

public class SettingsModel
{
    [DataMember(Name = "branch")]
    public IList<BranchInfo> Branches { get; set; }

    [IgnoreDataMember]
    public bool ShouldPromptForSettings => Branches.Count < 1;

    public SettingsModel()
    {
        Branches = new List<BranchInfo>();
    }

    public void CreateOrUpdateBranch(string name, string directoryPath)
    {
        var branch = GetBranch(name);

        if (branch == null)
        {
            branch = new BranchInfo
            {
                Name = name,
                DirectoryPath = directoryPath
            };

            Branches.Add(branch);
        }

        branch.Name = name;
        branch.DirectoryPath = directoryPath;
    }

    public BranchInfo? GetBranch(string name) =>
        Branches.FirstOrDefault(b => b.Name != null && b.Name.Equals(name));

    public void RemoveBranch(string name)
    {
        var index = (Branches as List<BranchInfo>)?.FindIndex(b => b.Name != null && b.Name.Equals(name)) ?? -1;

        if (index >= 0)
        {
            Branches.RemoveAt(index);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenDCSLauncher.Models;

public class BranchSettings
{
    public string? Name { get; set; }
    public string? DirectoryPath { get; set; }
}

public class SettingsModel
{
    [DataMember(Name = "branch")]
    public IList<BranchSettings> Branches { get; set; }

    public SettingsModel()
    {
        Branches = new List<BranchSettings>();
    }

    public void CreateOrUpdateBranch(string name, string directoryPath)
    {
        var branch = GetBranch(name);

        if (branch == null)
        {
            branch = new BranchSettings
            {
                Name = name,
                DirectoryPath = directoryPath
            };

            Branches.Add(branch);
        }

        branch.Name = name;
        branch.DirectoryPath = directoryPath;
    }

    public BranchSettings? GetBranch(string name) => Branches.FirstOrDefault(b => b.Name != null && b.Name.Equals(name));
}
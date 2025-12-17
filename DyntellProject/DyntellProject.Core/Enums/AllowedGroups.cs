using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyntellProject.Core.Enums;

public static class AllowedGroups
{
    public static AgeGroup[] GetAllowedAgeGroups(this PostType postType)
    {
        return postType switch
        {
            PostType.Gyermek => new[] { AgeGroup.Gyerek, AgeGroup.Felnott, AgeGroup.Idos },
            PostType.Kozelet => new[] { AgeGroup.Felnott, AgeGroup.Idos },
            PostType.Sport => new[] { AgeGroup.Felnott },
            _ => Array.Empty<AgeGroup>()
        };
    }
}

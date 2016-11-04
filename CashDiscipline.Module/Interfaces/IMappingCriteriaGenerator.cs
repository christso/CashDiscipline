using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Interfaces
{
    public interface IMappingCriteriaGenerator : IMapping
    {
        Type CriteriaObjectType { get; }
        string Criteria { get; set; }
    }
}

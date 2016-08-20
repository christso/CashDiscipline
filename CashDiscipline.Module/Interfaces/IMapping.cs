using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Interfaces
{
    public interface IMapping
    {
        int MapStep { get; }
        string CriteriaExpression { get; }

    }
}

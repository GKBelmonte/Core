using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ages
{
    public delegate IIndividual CrossOver(List<IIndividual > parents);
    /// <summary>
    /// Is A > B ? by how much? (+, o.w. - ) 
    /// </summary>
    /// <param name="left">Left individual</param>
    /// <param name="right">Right individual</param>
    /// <returns>A - B</returns>
    public delegate float Evaluate(IIndividual left, IIndividual right);
    public delegate void OnTournamentComplete(List<IIndividual> sortedPop);
}

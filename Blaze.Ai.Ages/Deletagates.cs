using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Ai.Ages
{
    public delegate IIndividual CrossOver(List<IIndividual > parents);
    /// <summary>
    /// Is A > B ? by how much? (+, o.w. - ) 
    /// </summary>
    /// <param name="left">Left individual</param>
    /// <param name="right">Right individual</param>
    /// <returns>A - B</returns>
    public delegate float CompareEvaluate(IIndividual left, IIndividual right);
    /// <summary>
    /// The value to minimize
    /// </summary>
    public delegate float Evaluate(IIndividual pers);
    public delegate void TournamentComplete(List<IIndividual> sortedPop);

    public delegate IIndividual Generate(Random r);
}

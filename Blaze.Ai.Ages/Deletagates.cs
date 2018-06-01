using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Ai.Ages
{
    /// <summary>
    /// Creates a new individual from the parents
    /// </summary>
    /// <param name="r">The GA's random generator. Should not be omitted to have deterministic runs</param>
    public delegate IIndividual CrossOver(List<IIndividual> parents, Random r);
    
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

    /// <summary>
    /// Generates a fresh new individual
    /// </summary>
    /// <param name="r">The GA's random generator. Should not be omitted to have deterministic runs</param>
    /// <returns></returns>
    public delegate IIndividual Generate(Random r);

    /// <summary>
    /// Calculates the difference in the genome of the individuals
    /// </summary>
    public delegate double Distance(IIndividual l, IIndividual r);
}

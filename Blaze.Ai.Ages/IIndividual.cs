using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ages
{
    public interface IIndividual
    {
        //IIndividual(IIndividual other, string name);
        //IIndividual(string name);
        void Regenerate();
        IIndividual Mutate(float probability, float sigma);
        string Name { get; }
    }
}

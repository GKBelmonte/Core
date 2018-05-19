using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AiLibrary
{
    public delegate List<object> FindAllPossibleMoves(object state);

    public delegate float GetFitnessForState(object state, object parameters);

    public delegate void OnMoveReady(object nextMove);

    public delegate void Prioritizer(List<object> moves, bool maximizing);

}

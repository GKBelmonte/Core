using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AiLibrary
{
    class Node
    {
        object mState;
        public object State
        {
            get { return mState; }
        }
        public float inheritedValue;
        public float trueValue;
        //public float alpha;
        //public float beta;
        public List<Node> children;
        
        public Node(object state)
        {
            trueValue = float.NaN;
            inheritedValue = float.NaN;
            children = new List<Node>();
            mState = state;
        }

    }

}

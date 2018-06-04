using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public class TriviumCypher : SaltedCypher
    {
        public TriviumCypher() : base(new StreamCypher(), 20) { }
    }
}

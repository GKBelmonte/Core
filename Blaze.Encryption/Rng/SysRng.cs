﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Rng
{
    public class SysRng : Random, IRng
    {
        public SysRng(int seed) : base(seed) { }
        public SysRng() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ages
{
    public static class Utils
    {
        /**********************************  Helping functions  **********************************/
        //Random integer between param1 inclusive and param2 exclusive
        public static int RandomInt (int p1, int p2)
        {
	        return rand.Next(p1,p2);
        }


        //Given a probability p, generates a random number, and
        //checks if the probability passes as true or false 
        //for one instance. 0<=p<1
        //true if pass, false if fail
        public static bool ProbabilityPass(float p)
        {
	        var dice = rand.NextDouble();
	        if( dice < p )
		        return true;
	        else
		        return false;
        }
        static Random rand = new Random();
        //Given a standard deviation s, return a random value around 0 with that standard deviation
        public static float GausianNoise(float s) 
        {
	        //3 random numbers from -1 to 1.
	        var d0 = rand.NextDouble()*2.0 - 1.0;
	        var d1 = rand.NextDouble()*2.0 - 1.0;
	        var d2 = rand.NextDouble()*2.0 - 1.0;
	        var d3 = rand.NextDouble()*2.0 - 1.0;
	        var d4 = rand.NextDouble()*2.0 - 1.0;
	        return (float)((d0 + d1 + d2 + d3 + d4)*s);
	
        }
    }
}

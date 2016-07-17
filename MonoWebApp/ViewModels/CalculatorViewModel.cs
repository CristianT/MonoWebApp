using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoWebApp
{
    public class CalculatorViewModel : BaseViewModel
    {
        public int Number1 { get; set; }

        public int Number2 { get; set; }

        public int Result { get; set; }

        public void Calculate()
        {
            Result = Number1 + Number2;
        }

        public override Task Init()
        {
            this.Number1 = 2;
            this.Number2 = 4;

            return base.Init();
        }
    }
}

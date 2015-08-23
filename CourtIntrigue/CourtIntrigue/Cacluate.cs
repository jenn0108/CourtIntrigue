using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Cacluate
    {
    }

    interface ICalculate
    {
        double Calculate(EventContext context, Game game);
    }

    class ConstantCalculate : ICalculate
    {
        private double constant;
        public ConstantCalculate(double constant)
        {
            this.constant = constant;
        }
        public double Calculate(EventContext context, Game game)
        {
            return constant;
        }
    }

    class VariableCalculate : ICalculate
    {
        private string varName;
        public VariableCalculate(string varName)
        {
            this.varName = varName;
        }
        public double Calculate(EventContext context, Game game)
        {
            return XmlHelper.GetTestValue(context, game, varName);
        }
    }

    class AddCalculate : ICalculate
    {
        private ICalculate[] parts;

        public AddCalculate(ICalculate[] parts)
        {
            this.parts = parts;
        }
        public double Calculate(EventContext context, Game game)
        {
            double result = 0.0;
            foreach(var part in parts)
            {
                result += part.Calculate(context, game);
            }
            return result;
        }
    }

    class SubtractCalculate : ICalculate
    {
        private ICalculate left;
        private ICalculate right;

        public SubtractCalculate(ICalculate left, ICalculate right)
        {
            this.left = left;
            this.right = right;
        }
        public double Calculate(EventContext context, Game game)
        {
            return left.Calculate(context, game) - right.Calculate(context, game);
        }
    }

    class NegateCalculate : ICalculate
    {
        private ICalculate inner;

        public NegateCalculate(ICalculate inner)
        {
            this.inner = inner;
        }
        public double Calculate(EventContext context, Game game)
        {
            return -inner.Calculate(context, game);
        }
    }

    class MultiplyCalculate : ICalculate
    {
        private ICalculate[] parts;

        public MultiplyCalculate(ICalculate[] parts)
        {
            this.parts = parts;
        }
        public double Calculate(EventContext context, Game game)
        {
            double result = 1.0;
            foreach (var part in parts)
            {
                result += part.Calculate(context, game);
            }
            return result;
        }
    }

    class DivideCalculate : ICalculate
    {
        private ICalculate left;
        private ICalculate right;

        public DivideCalculate(ICalculate left, ICalculate right)
        {
            this.left = left;
            this.right = right;
        }
        public double Calculate(EventContext context, Game game)
        {
            return left.Calculate(context, game) / right.Calculate(context, game);
        }
    }
}

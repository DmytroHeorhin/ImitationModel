using System.Numerics;
using MathNet.Numerics;
using System;
using System.Collections.Generic;

namespace ImitationModel
{
    public class NumberOfErrorsProbabilityCalculator
    {
        int _bitsPerBlock;
        BigInteger _bitsPerBlockFactorial;
        double _symbolErrrorRate;

        public NumberOfErrorsProbabilityCalculator(int bitsPerBlock, double symbolErrrorRate)
        {
            if (bitsPerBlock <= 0)
                throw new ArgumentException("Number of bits per block should be greater than zero.");
            //if (bitsPerBlock > 22)
            //    throw new ArgumentException("Number of bits per block should be not greater than 22.");

            _bitsPerBlock = bitsPerBlock;
            _bitsPerBlockFactorial = SpecialFunctions.Factorial((BigInteger)_bitsPerBlock);
            _symbolErrrorRate = symbolErrrorRate;
        }

        public int BitsPerBlock { get { return _bitsPerBlock; } }

        public double GetProbabilityOfOccuringErrorsInQuantity(int numberOfErrors)
        {
            if (numberOfErrors < 0)
                throw new ArgumentException("Number of errors should be not less than zero.");
            //if (numberOfErrors > 22)
            //    throw new ArgumentException("Number of errors should be not greater than 22.");

            var denominator = (SpecialFunctions.Factorial((BigInteger)numberOfErrors) * SpecialFunctions.Factorial(new BigInteger(_bitsPerBlock - numberOfErrors)));
            var power = _bitsPerBlock - numberOfErrors;
            var numerator = Math.Pow(_symbolErrrorRate, numberOfErrors) * Math.Pow(1 - _symbolErrrorRate, power);
            var result = _bitsPerBlockFactorial / denominator;
            return (double)result * numerator;
        }
    }

    public class NumberOfErrorsProbabilityScale
    {
        double[] _scale;
        double[] _probabilities;

        public NumberOfErrorsProbabilityScale(NumberOfErrorsProbabilityCalculator calculator, double accuracy)
        {
            List<double> scale = new List<double>();
            List<double> probabilities = new List<double>();
            double sum = 0;
            int numberOfErrors = 0;
            while (1 - sum > accuracy && numberOfErrors <= calculator.BitsPerBlock)
            {
                var probabability = calculator.GetProbabilityOfOccuringErrorsInQuantity(numberOfErrors);
                probabilities.Add(probabability);
                sum += probabability;
                scale.Add(sum);
                numberOfErrors++;
            }
            _scale = scale.ToArray();
            _probabilities = probabilities.ToArray();
        }

        public double[] Scale
        {
            get { return _scale; }
        }

        public double[] Probabilities
        {
            get { return _probabilities; }
        }

        public int NumberOfCases
        {
            get { return _scale.Length; }
        }
    }

    public class Experimentator
    {
        NumberOfErrorsProbabilityScale _scale;
        Random _randomizer;

        public Experimentator(NumberOfErrorsProbabilityScale scale)
        {
            _scale = scale;
            _randomizer = new Random();
        }      

        public int MakeExperiment_ReturnNumberOfErrors()
        {
            var randomNumber = _randomizer.NextDouble();
            int result = -1;
            for(int i = 0; i < _scale.NumberOfCases; i++)
            {
                if(_scale.Scale[i] > randomNumber)
                {
                    result = i;
                    break;
                }
            }           
            return result;
        }

        public int[] MakeExperiments_ReturnNumberOfErrors(int numberOfExperiments)
        {
            int[] result = new int[numberOfExperiments];
            for(int i = 0; i < numberOfExperiments; i++)
            {
                result[i] = MakeExperiment_ReturnNumberOfErrors();
            }
            return result;
        }
    }

    

    class Program
    {
        static void Main(string[] args)
        {
            var pSet = new NumberOfErrorsProbabilityCalculator(1000, 0.01);
            var pScale = new NumberOfErrorsProbabilityScale(pSet, 0.001);
            var experimentator = new Experimentator(pScale);

            foreach (int n in experimentator.MakeExperiments_ReturnNumberOfErrors(50000))
                Console.WriteLine(n);



            /*
            foreach (var p in pScale.Probabilities)
            {
                Console.WriteLine(p);
            }
            */


        }

        
    }
}

using System;
using System.Threading;

namespace DragonMarble
{
    public static class RandomFactory
    {
        private static int _seedCounter = new Random().Next();

        [ThreadStatic]
        private static Random _rng;

        public static Random NewRandom
        {
            get
            {
                if (_rng == null)
                {
                    int seed = Interlocked.Increment(ref _seedCounter);
                    _rng = new Random(seed);
                }
                return _rng;
            }
        }
    }
    public static class RandomUtil
    {
        public static int Next(int min, int max)
        {
            return RandomFactory.NewRandom.Next(min, max);
        }
        public static float Next(float min, float max)
        {
            return RandomFactory.NewRandom.Next((int)(min * 1000), (int)max * 1000) / 1000f;
        }
    }
}

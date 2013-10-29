using System;

namespace DragonMarble
{
    public static class RandomFactory
    {
        public static Random NewRandom()
        {
            return new Random();
        }
    }
    public static class RandomUtil
    {
        public static int Next(int min, int max)
        {
            return RandomFactory.NewRandom().Next(min, max);
        }
        public static float Next(float min, float max)
        {
            return RandomFactory.NewRandom().Next((int)(min * 1000), (int)max * 1000) / 1000f;
        }
    }
}

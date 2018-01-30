using AutoMapper;

namespace TurnerStarterApi.Core.Extensions
{
    public static class MapperExtensions
    {
        public static TOut To<TOut>(this object obj)
        {
            return Mapper.Map<TOut>(obj);
        }
    }
}

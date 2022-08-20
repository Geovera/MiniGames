using System.Collections.Generic;
using System.Linq;

namespace GV.Shared.Collections
{
     public static class ListExtensions
     {
          public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
          {
               return source.Select((item, index) => (item, index));
          }
     }
}
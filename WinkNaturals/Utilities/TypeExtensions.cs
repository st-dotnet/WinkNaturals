using System.IO;
using System.Runtime.Serialization;

namespace WinkNaturals.Utilities
{
    public static class TypeExtensions
    {
        public static T DeepClone<T>(this T source)
        {
            using (var stream = new MemoryStream())
            {
                var dcs = new DataContractSerializer(typeof(T));
                dcs.WriteObject(stream, source);
                stream.Position = 0;
                return (T)dcs.ReadObject(stream);
            }
        }
    }
}

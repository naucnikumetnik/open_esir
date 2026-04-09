using System.Collections;

namespace TaxCore.Libraries.Certificates.Extensions
{
    public static class ObjectExtensions
    {

        /// <summary>
        /// Reads all properties and returns <see cref="Dictionary"/> of <see cref="string"/> and string representation of it's values.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToStringDictionary(this object obj)
        {
            var result = new Dictionary<string, string>();

            var type = obj.GetType();

            foreach (var property in type.GetProperties())
            {
                var value = property.GetValue(obj);

                if (value != null)
                {
                    if (value is IList)
                        result.Add(property.Name, ((IList)value).Count > 0 ? ((IList)value)[0] as String : null);
                    else if (value.ToString() != string.Empty)
                        result.Add(property.Name, value.ToString());
                }

            }

            return result;

        }

        /// <summary>
        /// Creates dictionary with objects property names, as keys, and their values
        /// </summary>
        /// <param name="self">The object.</param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object self) 
            => self.GetType().GetProperties().ToDictionary(item => item.Name, item => item.GetValue(self));
    }
}

using Siemens.Engineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiaMcpServer.ModelContextProtocol
{
    internal class Helper
    {
        public static List<Attribute> GetAttributeList(IEngineeringObject obj)
        {
            var attributes = new List<Attribute>();
            foreach (var attr in obj.GetAttributeInfos())
            {
                object value = obj.GetAttribute(attr.Name);
                attributes.Add(new Attribute
                {
                    Name = attr.Name,
                    Value = value,
                    AccessMode = Enum.GetName(typeof(EngineeringAttributeAccessMode), attr.AccessMode)
                });
            }

            return attributes;
        }
    }
}

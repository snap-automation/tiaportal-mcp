using Siemens.Engineering;
using System;
using System.Collections.Generic;

namespace TiaMcpServer.ModelContextProtocol
{
    internal class Helper
    {
        public static List<Attribute> GetAttributeList(IEngineeringObject obj)
        {
            var attributes = new List<Attribute>();

            if (obj != null)
            {
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
            }

            return attributes;
        }
    }
}

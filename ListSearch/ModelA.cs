using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace ListSearch
{
    public class ModelA: IEquatable<ModelA>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<ModelA> SonModelA { get; set; } = new List<ModelA>();

        public bool Equals([AllowNull] ModelA other)
        {
            if(other is null) 
            {
                return false;
            }
            var ans = true;
            if (this.Id == other.Id && this.Name == other.Name && this.SonModelA.Count == other.SonModelA.Count ) 
            {
                for(var i =0; i< this.SonModelA.Count; i++) 
                {
                    ans = ans && this.SonModelA[i].Equals(other.SonModelA[i]);
                }
            }
            else 
            {
                ans = false;
            }
            return ans;
        }
        public override bool Equals(object obj) => Equals(obj as ModelA);
        public override int GetHashCode() => (Id,Name,SonModelA).GetHashCode();

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceQ
{
    class Position
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public override string ToString()
        {
            return String.Format("[Top:{0} Left:{1}]", Top, Left);
        }

        public static Position operator+ (Position a, Position b)
        {
            Position p = new Position
            {
                Top = a.Top + b.Top,
                Left = a.Left + b.Left
            };
            return p;
        }
    }
}

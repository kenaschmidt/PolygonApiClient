using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient.Models
{
    //
    // Portfolio class represents a collection of securities and provides properties and methods to do various calculations
    //

    public class Portfolio
    {
        public List<Position> Positions { get; } = new List<Position>();
        public Portfolio()
        {
        }
        public void AddPosition(Position position)
        {
            Positions.Add(position);
        }
    }

}

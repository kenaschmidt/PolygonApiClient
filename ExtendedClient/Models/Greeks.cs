using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class Greeks
    {
        public DateTime AsOf { get; }

        public double IV { get; }
        public double Delta { get; }
        public double Gamma { get; }
        public double Theta { get; }
        public double Vega { get; }
        public double Vanna { get; }
        public double Veta { get; }
        public double Charm { get; }
        public double Vomma { get; }
        public double Zomma { get; }
        public double Speed { get; }
        public double Color { get; }
        public double Lambda { get; }

        public Greeks(DateTime asOf, double iv, double delta, double gamma, double theta, double vega, double vanna, double veta, double charm, double vomma, double zomma, double speed, double color, double lambda)
        {
            AsOf = asOf;
            IV = iv;
            Delta = delta;
            Gamma = gamma;
            Theta = theta;
            Vega = vega;
            Vanna = vanna;
            Veta = veta;
            Charm = charm;
            Vomma = vomma;
            Zomma = zomma;
            Speed = speed;
            Color = color;
            Lambda = lambda;
        }

        public override string ToString()
        {

            return $"AsOf: {AsOf.ToString()}\r\n" +
                $"IV: {IV:0.000%}\r\n" +
                $"Delta: {Delta:0.000}\r\n" +
                $"Gamma: {Gamma:0.000}\r\n" +
                $"Theta: {Theta:0.000}\r\n" +
                $"Vega: {Vega:0.000}\r\n" +
                $"Vanna: {Vanna:0.000}\r\n" +
                $"Veta: {Veta:0.000}\r\n" +
                $"Charm: {Charm:0.000}\r\n" +
                $"Vomma: {Vomma:0.000}\r\n" +
                $"Zomma: {Zomma:0.000}\r\n" +
                $"Speed: {Speed:0.000}\r\n" +
                $"Color: {Color:0.000}\r\n" +
                $"Lambda: {Lambda:0.000}\r\n";
        }
    }
}

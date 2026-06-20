using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Rotator_Interface
{
    public class SerialDriver
    {
        public int cas1 = 500;
        public int cas5 = 800;
        public int cas10 = 1200;

        public float aktualnyUhol = 0f;
        public event Action<float> ChangeDegree;

        SerialPort port;
        CancellationTokenSource zrus;

        public void Otvor()
        {
            port = new SerialPort("COM17", 9600, Parity.None, 8, StopBits.One);
            port.Open();
        }

        public void Zatvor()
        {
            Stopni();
            if (port != null && port.IsOpen) port.Close();
        }

        public void IdiNa(float kam)
        {
            Stopni();
            zrus = new CancellationTokenSource();
            CancellationToken t = zrus.Token;
            Task.Run(() => {
                try { Tocsa(kam, t); }
                catch { }
            });
        }

        void Stopni()
        {
            if (zrus != null) { zrus.Cancel(); zrus = null; }
        }

        void Tocsa(float kam, CancellationToken t)
        {
            kam = Uprav(kam);
            while (!t.IsCancellationRequested)
            {
                float rozdiel = kam - aktualnyUhol;
                if (rozdiel > 180) rozdiel -= 360;
                if (rozdiel < -180) rozdiel += 360;

                if (Math.Abs(rozdiel) < 0.5f) break;

                char prikaz;
                float krok;
                int cakaj;

                if (Math.Abs(rozdiel) >= 10)
                {
                    prikaz = rozdiel > 0 ? 'C' : 'c';
                    krok = rozdiel > 0 ? 10f : -10f;
                    cakaj = cas10;
                }
                else if (Math.Abs(rozdiel) >= 5)
                {
                    prikaz = rozdiel > 0 ? 'B' : 'b';
                    krok = rozdiel > 0 ? 5f : -5f;
                    cakaj = cas5;
                }
                else
                {
                    prikaz = rozdiel > 0 ? 'A' : 'a';
                    krok = rozdiel > 0 ? 1f : -1f;
                    cakaj = cas1;
                }

                port.Write(new char[] { prikaz }, 0, 1);
                aktualnyUhol = Uprav(aktualnyUhol + krok);
                if (ChangeDegree != null) ChangeDegree(aktualnyUhol);
                Thread.Sleep(cakaj);
            }
        }

        float Uprav(float az)
        {
            az = az % 360f;
            if (az < 0) az += 360f;
            return az;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turbo_Racer_Alfa1.Addons
{
    class MusicPlayer
    {
        const int Rest = 0;
        // Helmet - Better Notes
        const int E3 = 164, F3 = 174, G3 = 196, A3 = 220, B3 = 246;
        // Deftones - My Own Summer Notes
        const int CSharp3 = 139, D3 = 147, GSharp2 = 104, G2 = 98;

        public static void PlayHelmetBetter()
        {
            int[,] songData = {
            { E3, 200 }, { E3, 200 }, { Rest, 100 }, { E3, 200 },
            { F3, 200 }, { F3, 200 }, { Rest, 100 }, { E3, 200 },
            { G3, 300 }, { F3, 200 }, { E3, 400 }, { Rest, 200 },
            { E3, 150 }, { E3, 150 }, { A3, 150 }, { G3, 150 },
            { E3, 150 }, { E3, 150 }, { A3, 150 }, { G3, 150 }
        };
            PlayLoop(songData);
        }

        public static void PlayDeftones()
        {
            // Frequency Definitions
            const int B2 = 123, CSharp3 = 139, DSharp3 = 156, E3 = 165, FSharp3 = 185, GSharp3 = 208;
            const int ASharp3 = 233, B3 = 247, CSharp4 = 277, DSharp4 = 311, E4 = 330, FSharp4 = 370;
            const int Rest = 0;

            int[,] songData = {
        // --- INTRO / VERSE (The Ambient Pulse) ---
        // Bass Note | Melodic Echo 1 | Melodic Echo 2
        { B2, 800 }, { FSharp3, 200 }, { DSharp3, 200 }, { FSharp3, 200 },
        { B2, 600 }, { FSharp3, 200 }, { GSharp3, 400 }, { Rest, 100 },

        { E3, 800 }, { B3, 200 }, { GSharp3, 200 }, { B3, 200 },
        { E3, 600 }, { B3, 200 }, { FSharp3, 400 }, { Rest, 100 },

        // --- PRE-CHORUS (The "Floating" buildup) ---
        { GSharp3, 600 }, { FSharp3, 600 }, { E3, 800 }, { Rest, 200 },
        { GSharp3, 600 }, { ASharp3, 600 }, { B3, 800 }, { CSharp4, 400 },

        // --- CHORUS (Chino's Soaring Melody) ---
        // "Tonight... I feel... like more..."
        { B3, 1000 }, { CSharp4, 400 }, { DSharp4, 1200 }, { Rest, 100 },
        { DSharp4, 200 }, { E4, 800 }, { DSharp4, 400 }, { B3, 1200 },
        
        // The High Ambient Lead (The "Sparkle" in the song)
        { FSharp4, 600 }, { E4, 300 }, { DSharp4, 300 }, { CSharp4, 600 },
        { B3, 1000 }, { Rest, 300 },

        // --- THE "UNDERWATER" BRIDGE ---
        // Very slow, deep frequencies
        { B2, 1200 }, { Rest, 100 }, { CSharp3, 1200 }, { Rest, 100 },
        { DSharp3, 1800 }, { Rest, 400 },

        // --- FINAL OUTRO ECHO ---
        { B2, 2000 }
    };

            PlayLoop(songData);
        }
        private static void PlayLoop(int[,] songData)
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < songData.GetLength(0); i++)
                    {
                        if (songData[i, 0] == Rest) Thread.Sleep(songData[i, 1]);
                        else Console.Beep(songData[i, 0], songData[i, 1]);
                    }
                }
            }
            catch (ThreadInterruptedException) { }
        }
    }
}

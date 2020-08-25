using System;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [Flags]
    [Serializable]
    public enum PsfBitness
    {
        Neutral = 1,
        x86 = 2,
        x64 = 4
    }
}
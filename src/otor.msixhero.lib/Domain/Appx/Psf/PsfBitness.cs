using System;

namespace otor.msixhero.lib.Domain.Appx.Psf
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
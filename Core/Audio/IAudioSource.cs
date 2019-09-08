using System;
using System.Numerics;

namespace Helion.Audio
{
    /// <summary>
    /// A source of audio that can be played.
    /// </summary>
    /// <remarks>
    /// Supports a position and velocity so that we can attach sounds to actors
    /// in a world. This will be interpolated by the implementation to give us
    /// certain effects that we wouldn't have otherwise.
    /// </remarks>
    public interface IAudioSource : IDisposable
    {
        /// <summary>
        /// The location this audio source is to be played it. This is in world
        /// coordinates.
        /// </summary>
        Vector3 Position { get; set; }
        
        /// <summary>
        /// The velocity (in map units) of the audio source.
        /// </summary>
        Vector3 Velocity { get; set; }
        
        /// <summary>
        /// Starts playing the sound.
        /// </summary>
        void Play();
        
        /// <summary>
        /// Stops playing the sound.
        /// </summary>
        void Stop();
    }
}
/*

    Copyright (C) 2020 francescomesianodev

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

*/

using UnityEngine;

namespace UnityFourier
{
    /// <summary>
    /// Allows to manage and retrive audio data
    /// </summary> 
    public class AudioPeer : MonoBehaviour
    {
        /// <summary>
        /// Instance that allows to access class public methods and variables
        /// </summary>
        public static AudioPeer SharedInstance;

        /// <summary>
        /// Defines a channel where the sample data will be taken
        /// </summary>
        public enum AudioChannel 
        {
            All,
            Left,
            Right
        }

        [Header("AudioPeer - Settings")]
        [Space(2f)]

        [Header("MonoBehaviour Related")]
        [Space(1f)]

        [Tooltip("How many frames before the next update?")]
        /// <summary>
        /// Defines how much frames should be skipped before the next update
        /// </summary>
        public int frameInterval = 0;

        [Header("Bands Related")]
        [Space(1f)]

        [Tooltip("Expands the audio bands to a maximum of 64 allowing more data income at the cost of performance")]
        /// <summary>
        /// Expands the audio bands to a maximum of 64 allowing more data income at the cost of performance
        /// </summary>
        public bool augmentBands = false;

        [Header("Samples Related")]
        [Space(1f)]

        [Tooltip("Defines how the spectrum data should be taken")]
        /// <summary>
        /// Defines how the spectrum data should be taken
        /// </summary>
        public FFTWindow spectrumWindow = FFTWindow.Blackman;

        [Tooltip("How many samples should we generate?")]
        /// <summary>
        /// Total samples that should be taken, this won't be changeable during runtime
        /// </summary>
        public int samplesAmount;

        [Tooltip("The channel where the sample data will be taken")]
        /// <summary>
        /// The channel where the sample data will be taken
        /// </summary>
        public AudioChannel samplesChannel;

        [Header("Frequency Related")]
        [Space(1f)]

        [Tooltip("Multiplies the frequency value")]
        /// <summary>
        /// Sometimes the outgoing frequency value can be low, this value can help to make it higher and give better results for visual purposes
        /// </summary>
        public float frequencyMultiplier = 10f;

        [Header("Audio Profile Related")]
        [Space(1f)]

        [Tooltip("More higher is the profile and more higher the values will be, this will give smooth result in some cases")]
        /// <summary>
        /// More higher is the profile and more higher the values will be, this will give smooth result in some cases
        /// </summary>
        public float audioProfileIndex;

        private AudioSource _targetSource;                                                                                          // target source where the data will be taken

        /* Pre-defined variables for the audio data that should be gathered */
        private float[] _lSamples, _rSamples;                                                                                       // left and right samples for stereo channels

        private float[] _localBufferedBands;                                                                                        // used to calculate the general bands       

        private float[] _audioBands, _bufferedAudioBands, _frequencyBands;                                                          // frequency and buffered audio bands
        private float[] _highRangeFrequencies, _bufferedDecreasingFactors;                                                          // high range values that deriver from the defined bands

        private float _amplitudeFactor, _bufferedAmplitudeFactor, _highRangeAmplitudeFactor;                                        // amplitude related factors

        private int _currentSample = 0, _currentFrequencyBand = 0;                                                                  // current samples and frequency bands that the system is aiming

        private float _currentFrequencyAverageResult = 0.0f;                                                                        // current average frequency
        private float _currentAmplitude = 0.0f, _currentBufferedAmplitude = 0.0f;                                                   // current amplitude values

        private int _currentSamplePowValue = 0;                                                                                     // current sample power value, used only when augmentedBands is true

        /// <summary>
        /// Gets a floating point array wich stores the left channel samples value
        /// </summary>
        public float[] LeftChannelSamples { get => _lSamples; }
        /// <summary>
        /// Gets a floating point array wich stores the right channel samples value
        /// </summary>
        public float[] RightChannelSamples { get => _rSamples; }

        /// <summary>
        /// Gets a floating point array wich stores the buffered channel band values
        /// </summary>
        public float[] LocalBufferedBands { get => _localBufferedBands; }

        /// <summary>
        /// Gets a floating point array wich stores the unbuffered bands
        /// </summary>
        public float[] AudioBands { get => _audioBands; }
        /// <summary>
        /// Gets a floating point array wich stores the unbuffered bands
        /// </summary>
        public float[] BufferedAudioBands { get => _bufferedAudioBands; }
        /// <summary>
        /// Gets a floating point array wich stores the unbuffered frequency bands
        /// </summary>
        public float[] FrequencyBands { get => _frequencyBands; }

        /// <summary>
        /// Gets a floating point array wich stores the unbuffered high range frequency values
        /// </summary>
        public float[] HighRangeFrequencies { get => _highRangeFrequencies; }
        /// <summary>
        /// Gets a floating point array wich stores each audio buffer decreasing factor value
        /// </summary>
        public float[] BufferedDecreasingFactors { get => _bufferedDecreasingFactors; }

        /// <summary>
        /// Gets a floating point value that defines the amplitude factor
        /// </summary>
        public float AmplitudeFactor { get => _amplitudeFactor; }
        /// <summary>
        /// Gets a floating point value wich stores the buffered amplitude factor
        /// </summary>
        public float BufferedAmplitudeFactor { get => _bufferedAmplitudeFactor; }
        /// <summary>
        /// Gets a floating point value wich stores the high range amplitude factor
        /// </summary>
        public float HighRangeAmplitudeFactor { get => _highRangeAmplitudeFactor; }

        /// <summary>
        /// Current sample that the peer is aiming
        /// </summary>
        public int CurrentSample { get => _currentSample; }

        /// <summary>
        /// Current frequency band that the peer is aiming
        /// </summary>
        public int CurrentFrequencyBand { get => _currentFrequencyBand; }
        /// <summary>
        /// Current average frequency from the current frequency band that the peer is aiming</see>
        /// </summary>
        public float CurrentFrequencyAverageResult { get => _currentFrequencyAverageResult; }

        /// <summary>
        /// Current amplitude value
        /// </summary>
        public float CurrentAmplitude { get => _currentAmplitude; }
        /// <summary>
        /// Current buffered amplitude value
        /// </summary>
        public float CurrentBufferedAmplitude { get => _currentBufferedAmplitude; }

        /// <summary>
        /// Gets the current sample power if augmentBands is true
        /// </summary>
        public float CurrentSamplePower { get => _currentSamplePowValue; }

        /// <summary>
        /// Gets or Sets the target audio source where the data should be taken
        /// </summary>
        public AudioSource TargetSource { get => _targetSource; set => _targetSource = value; }

        private void Awake() 
        {
            if(SharedInstance == null)
                SharedInstance = this;

            _lSamples = new float[samplesAmount];
            _rSamples = new float[samplesAmount];

            if(augmentBands)
            {
                _localBufferedBands = new float[64];

                _audioBands = new float[64];
                _bufferedAudioBands = new float[64];
                _frequencyBands = new float [64];

                _highRangeFrequencies = new float[64];
                _bufferedDecreasingFactors = new float[64];
            }
            else 
            {
                _localBufferedBands = new float[8];

                _audioBands = new float[8];
                _bufferedAudioBands = new float[8];
                _frequencyBands = new float [8];

                _highRangeFrequencies = new float[8];
                _bufferedDecreasingFactors = new float[8];
            }
        }

        private void Start() 
        {
            if(_targetSource == null)
                _targetSource = GetComponent<AudioSource>();
            FetchProfile(audioProfileIndex, augmentBands);
        }

        private void Update() 
        {
            if(frameInterval != 0)
            {
                if(Time.frameCount % frameInterval == 0)
                {
                    GetSamplesData(spectrumWindow);

                    GenerateFrequencyBands(augmentBands);
                    GenerateBufferedBands(augmentBands);
                    GenerateAudioBands(augmentBands);

                    FetchAmplitude(augmentBands);
                }
            }
        }

        /// <summary>
        /// Gets and sets the current sample data using the AudioSource derived method <see cref="UnityEngine.AudioSource"/> <code>GetSpectrumData(float[] samples, int channel, FTTWindow window)</code>
        /// </summary>
        /// <param name="method">The FFTWindow function wich defines how to take the spectrum data</param>
        public virtual void GetSamplesData(FFTWindow method = FFTWindow.Blackman)
        {
            _targetSource.GetSpectrumData(_lSamples, 0, method);
            _targetSource.GetSpectrumData(_rSamples, 1, method);     
        }

        /// <summart>
        /// Generates the frequency bands
        /// </summary>
        /// <param name="augmentBands">Allows to extend from 8 bands to 64</param>
        public virtual void GenerateFrequencyBands(bool augmentBands = true)
        {
            if(augmentBands)
            {
                _currentFrequencyBand = 0;
                _currentSample = 1;
                _currentSamplePowValue = 0;

                for(int i = 0; i < 64; i++)
                {
                    _currentFrequencyAverageResult = 0.0f;

                    _currentSample = (int) Mathf.Pow(2, i) * 2;

                    if(i == 16 || i == 32 || i == 40 || i == 48 || i == 46)
                    {
                        _currentSamplePowValue++;
                        _currentSample += (int) Mathf.Pow(2, _currentSamplePowValue);

                        if(_currentSamplePowValue == 3)
                            _currentSample += 2;
                    }

                    for(int j = 0; j < _currentSample; j++)
                    {
                        if(samplesChannel == AudioChannel.All)
                            _currentFrequencyAverageResult += (_lSamples[_currentFrequencyBand] + _rSamples[_currentFrequencyBand]) * (_currentFrequencyBand + 1);
                        
                        if(samplesChannel == AudioChannel.Left)
                            _currentFrequencyAverageResult += _lSamples[_currentFrequencyBand] * (_currentFrequencyBand + 1);

                        if(samplesChannel == AudioChannel.Right)
                            _currentFrequencyAverageResult += _rSamples[_currentFrequencyBand] * (_currentFrequencyBand + 1);

                        _currentFrequencyBand++;
                    }

                    _currentFrequencyAverageResult /= _currentFrequencyBand;

                    if(frequencyMultiplier != 0)
                        _frequencyBands[i] = _currentFrequencyAverageResult * frequencyMultiplier;
                    else 
                        _frequencyBands[i] = _currentFrequencyAverageResult;
                }
            }
            else 
            {
                _currentFrequencyBand = 0;
                for(int i = 0; i < 8; i++)
                {
                    _currentFrequencyAverageResult = 0.0f;

                    _currentSample = (int) Mathf.Pow(2, i) * 2;

                    if(i == 7)
                        _currentSample += 2;

                    for(int j = 0; j < _currentSample; j++)
                    {
                        if(samplesChannel == AudioChannel.All)
                            _currentFrequencyAverageResult += (_lSamples[_currentFrequencyBand] + _rSamples[_currentFrequencyBand]) * (_currentFrequencyBand + 1);
                        
                        if(samplesChannel == AudioChannel.Left)
                            _currentFrequencyAverageResult += _lSamples[_currentFrequencyBand] * (_currentFrequencyBand + 1);

                        if(samplesChannel == AudioChannel.Right)
                            _currentFrequencyAverageResult += _rSamples[_currentFrequencyBand] * (_currentFrequencyBand + 1);

                        _currentFrequencyBand++;
                    }

                    _currentFrequencyAverageResult /= _currentFrequencyBand;

                    if(frequencyMultiplier != 0)
                        _frequencyBands[i] = _currentFrequencyAverageResult * frequencyMultiplier;
                    else 
                        _frequencyBands[i] = _currentFrequencyAverageResult;
                }
            }
        }

        /// <summart>
        /// Generates the buffered audio bands
        /// </summary>
        /// <param name="augmentBands">Allows to extend from 8 bands to 64</param>
        public virtual void GenerateBufferedBands(bool augmentBands = true)
        {
            if(augmentBands)
            {
                for(int i = 0; i < 64; ++i)
                {
                    if(_frequencyBands[i] > _localBufferedBands[i])
                    {
                        _localBufferedBands[i] = _frequencyBands[i];
                        _bufferedDecreasingFactors[i] = 0.005f;
                    }
                    
                    if(_frequencyBands[i] < _localBufferedBands[i])
                    {
                        _localBufferedBands[i] -= _bufferedDecreasingFactors[i];
                        _bufferedDecreasingFactors[i] *= 1.2f;
                    }
                }
            }
            else 
            {
                for(int i = 0; i < 8; ++i)
                {
                    if(_frequencyBands[i] > _localBufferedBands[i])
                    {
                        _localBufferedBands[i] = _frequencyBands[i];
                        _bufferedDecreasingFactors[i] = 0.005f;
                    }
                    
                    if(_frequencyBands[i] < _localBufferedBands[i])
                    {
                        _localBufferedBands[i] -= _bufferedDecreasingFactors[i];
                        _bufferedDecreasingFactors[i] *= 1.2f;
                    }
                }
            }
        }

        /// <summart>
        /// Generates the audio bands
        /// </summary>
        /// <param name="augmentBands">Allows to extend from 8 bands to 64</param>
        public virtual void GenerateAudioBands(bool augmentBands = true)
        {
            if(augmentBands)
            {
                for(int i = 0; i < 64; i++)
                {
                    if(_frequencyBands[i] > _highRangeFrequencies[i])
                    {
                        _highRangeFrequencies[i] = _frequencyBands[i]; 
                    }
                    _audioBands[i] = (_frequencyBands[i] / _highRangeFrequencies[i]);
                    _bufferedAudioBands[i] = (_localBufferedBands[i] / _highRangeFrequencies[i]);
                }
            } 
            else 
            {
                for(int i = 0; i < 8; i++)
                {
                    if(_frequencyBands[i] > _highRangeFrequencies[i])
                    {
                        _highRangeFrequencies[i] = _frequencyBands[i]; 
                    }
                    _audioBands[i] = (_frequencyBands[i] / _highRangeFrequencies[i]);
                    _bufferedAudioBands[i] = (_localBufferedBands[i] / _highRangeFrequencies[i]);
                }
            }
        }

        /// <summart>
        /// Fetches each band amplitude
        /// </summary>
        /// <param name="augmentBands">Allows to extend from 8 bands to 64</param>
        public virtual void FetchAmplitude(bool augmentBands = true)
        {
            if(augmentBands)
            {
                for(int i = 0; i < 64; i++)
                {
                    _currentAmplitude += _localBufferedBands[i];
                    _currentBufferedAmplitude += _bufferedAudioBands[i];
                }
            }
            else 
            {
                for(int i = 0; i < 8; i++)
                {
                    _currentAmplitude += _localBufferedBands[i];
                    _currentBufferedAmplitude += _bufferedAudioBands[i];
                }
            }

            if(_currentAmplitude > _highRangeAmplitudeFactor)
                _highRangeAmplitudeFactor = _currentAmplitude;
                
            _amplitudeFactor = _currentAmplitude / _highRangeAmplitudeFactor;
            _bufferedAmplitudeFactor = _currentAmplitude / _highRangeAmplitudeFactor;
        }

        /// <summart>
        /// Fetches the audio profile based on the given index
        /// </summary>
        /// <param name="index">Index of the profile, a higher index means higher values</param>
        /// <param name="augmentBands">Allows to extend from 8 bands to 64</param>
        public virtual void FetchProfile(float index, bool augmentBands = true)
        {
            if(augmentBands)
            {
                for(int i = 0; i < 64; i++)
                {
                    _highRangeFrequencies[i] = index;
                }
            }
            else 
            {
                for(int i = 0; i < 8; i++)
                {
                    _highRangeFrequencies[i] = index;
                }
            }
        }
    }
}

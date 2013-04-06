using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Renderer.SceneTools.Entities.Particles
{
    public struct ParticleEmitterSettings
    {
        #region Properties

        /// <summary>
        /// Is set to true if this ParticleEmitterSettings object has been completly initialized.
        /// </summary>
        public bool Initialized
        {
            get;
            set;
        }

        /// <summary>
        /// The radius in wold units that a particle can emit from. Depends on VaryX, VaryY and VaryZ.
        /// </summary>
        public int Radius
        {
            get;
            set;
        }

        /// <summary>
        /// The time, in seconds between each particle emission.
        /// </summary>
        public float IntervalBetweenParticles
        {
            get;
            set;
        }
        
        /// <summary>
        /// If true, the starting position of particles will vary from -radius to +radius on the X axis.
        /// </summary>
        public bool VaryX
        {
            get;
            set;
        }
        /// <summary>
        /// If true, the starting position of particles will vary from -radius to +radius on the Y axis.
        /// </summary>
        public bool VaryY
        {
            get;
            set;
        }
        /// <summary>
        /// If true, the starting position of particles will vary from -radius to +radius on the Z axis.
        /// </summary>
        public bool VaryZ
        {
            get;
            set;
        }

        /// <summary>
        /// If true, this emitter will only stop manually
        /// </summary>
        public bool Continuous
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of time to emit particles
        /// </summary>
        public float LifeSpan
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the Particle System loaded in the RendererAssetPool
        /// </summary>
        public string ParticleSystemName
        {
            get;
            set;
        }

        #endregion

        #region Load

        /// <summary>
        /// Loads the properties for this emitter from an input stream.
        /// </summary>
        /// <param name="reader">The input stream to load from.</param>
        /// <returns>True if initailized</returns>
        public bool Load(StreamReader reader)
        {
            try
            {
                Initialized = false;
                while (!reader.EndOfStream)
                {
                    if (reader.Peek() == '#') reader.ReadLine();
                    else
                    {
                        string[] line = reader.ReadLine().Split(":".ToCharArray());
                        line[0] = line[0].Trim(); line[1] = line[1].Trim();
                        line[1] = line[1].Replace(" ", ""); line[1] = line[1].Replace("[", ""); line[1] = line[1].Replace("]", "");
                        switch (line[0])
                        {
                            case "Radius":
                                Radius = Convert.ToInt32(line[1]);
                                break;
                            case "TimeBetweenParticles":
                                IntervalBetweenParticles = Convert.ToSingle(line[1]);
                                break;
                            case "LifeSpan":
                                LifeSpan = Convert.ToSingle(line[1]);
                                break;
                            case "VaryX":
                                if (line[1].Equals("Yes")) VaryX = true;
                                else VaryX = false;
                                break;
                            case "VaryY":
                                if (line[1].Equals("Yes")) VaryY = true;
                                else VaryY = false;
                                break;
                            case "VaryZ":
                                if (line[1].Equals("Yes")) VaryZ = true;
                                else VaryZ = false;
                                break;
                            case "Continuous": 
                                if(line[1].Equals("Yes")) Continuous = true;
                                else Continuous = false;
                                break;
                            case "ParticleSystemName":
                                ParticleSystemName = line[1];
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            Initialized = true;
            return true;
        }

        #endregion


    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Renderer.SceneTools.Entities.Particles
{
    public struct ParticleSystemSettings
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
        /// The name of the texture to load.
        /// </summary>
        public Texture2D Texture
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum number of particles to be stored in the system
        /// </summary>
        public int Capacity
        {
            get;
            set;
        } // Used by: ParticleSystem.cs

        /// <summary>
        /// How long the particles will last in seconds
        /// </summary>
        public TimeSpan Duration
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        public float DurationRandomness
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        
        /// <summary>
        /// The direction and magnatude of the worlds global force.
        /// </summary>
        public Vector3 GlobalForce
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The minimum force sensitivity that each particle will have. 
        /// This is a number from 0 to 1 representing the percentage of the Global Force to use.
        /// </summary>
        public float MinForceSensitivity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The minimum force sensitivity that each particle will have. 
        /// This is a number from 0 to 1 representing the percentage of the Global Force to use.
        /// </summary>
        public float MaxForceSensitivity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx

        /// <summary>
        /// A number between 0 and 1 indicating the percentage of the emitters velocity that will transfer to the particle.
        /// </summary>
        public float InertiaSensitivity
        {
            get;
            set;
        }// Used by: ParticleSystem.cs

        /// <summary>
        /// If true, direction will be set automatically from the rotation matrix of the parent emitter;
        /// </summary>
        public bool DirFromRotation
        {
            get;
            set;
        }// Used by: ParticleSystem.cs

        /// <summary>
        /// The speed at which a particle will travel in its initial direction
        /// </summary>
        public float InitalSpeed
        {
            get;
            set;
        }// Used by: ParticleSystem.cs

        /// <summary>
        /// The initial direction that a particle will travel
        /// </summary>
        public Vector3 Direction
        {
            get;
            set;
        }// Used by: ParticleSystem.cs

        public float MinStartHorizontalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        public float MaxStartHorizontalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        public float MinStartVerticalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        public float MaxStartVerticalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx

        /// <summary>
        /// The Minimum XY velocity that the particle will have upon expiration.
        /// </summary>
        public float MinEndHorizontalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The Minimum Z velocity that the particle will have upon expiration.
        /// </summary>
        public float MaxEndHorizontalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        public float MinEndVerticalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        public float MaxEndVerticalVelocity
        {
            get;
            set;
        }// Used by: ParticleEffect.fx

        #region Color
        /// <summary>
        /// The minimum color to tint the texture. White for none.
        /// </summary>
        public Color MinColor
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The maximum color value to tint the texture.
        /// </summary>
        public Color MaxColor
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        #endregion

        #region Size
        /// <summary>
        /// The minimum particle start size.
        /// </summary>
        public float MinStartSize
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The maximum particle start size.
        /// </summary>
        public float MaxStartSize
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The minimum particle end size.
        /// </summary>
        public float MinEndSize
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The maximum particle end size.
        /// </summary>
        public float MaxEndSize
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        #endregion

        #region Rotation
        /// <summary>
        /// The minimum particle rotation speed. Set equal to zero for no rotation.
        /// </summary>
        public float MinRotateSpeed
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        /// <summary>
        /// The maximum particle rotation speed. Set equal to zero for no rotation.
        /// </summary>
        public float MaxRotateSpeed
        {
            get;
            set;
        }// Used by: ParticleEffect.fx
        #endregion

        #region Blend Settings
        /// <summary>
        /// The Particle System's Source Blend.
        /// </summary>
        public Blend SourceBlend
        {
            get;
            set;
        }// Used by: ParticleSystem.cs
        /// <summary>
        /// The Particle System's destination blend.
        /// </summary>
        public Blend DestinationBlend
        {
            get;
            set;
        }// Used by: ParticleSystem.cs
        #endregion

        #endregion

        #region Load
        /// <summary>
        /// Loads all properties from an input stream.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <returns>True if initialized.</returns>
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
                        if (line.Length == 2)
                        {
                            line[0] = line[0].Trim(); line[1] = line[1].Trim();
                            line[1] = line[1].Replace(" ", ""); line[1] = line[1].Replace("[", ""); line[1] = line[1].Replace("]", "");
                            switch (line[0])
                            {
                                case "Texture":
                                    if (RendererAssetPool.Particles.ContainsKey(line[1]))
                                    {
                                        Texture = RendererAssetPool.Particles[line[1]];
                                    }
                                    else { Texture = RendererAssetPool.Particles["sample"]; }
                                    break;
                                case "Capacity":
                                    Capacity = Convert.ToInt32(line[1]);
                                    break;
                                case "Duration":
                                    Duration = TimeSpan.FromSeconds(Convert.ToSingle(line[1]));
                                    break;
                                case "DurationRandomness":
                                    DurationRandomness = Convert.ToSingle(line[1]);
                                    break;
                                case "GlobalForce":
                                    string[] vector = line[1].Split(",".ToCharArray());
                                    GlobalForce = new Vector3(Convert.ToSingle(vector[0]),
                                                              Convert.ToSingle(vector[1]),
                                                              Convert.ToSingle(vector[2]));
                                    break;
                                case "MinForceSensitivity":
                                    MinForceSensitivity = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxForceSensitivity":
                                    MinForceSensitivity = Convert.ToSingle(line[1]);
                                    break;
                                case "InertiaSensitivity":
                                    InertiaSensitivity = Convert.ToSingle(line[1]);
                                    break;
                                case "Direction":
                                    string[] initVelo = line[1].Split("[, ]".ToCharArray());
                                    Direction = new Vector3(Convert.ToSingle(initVelo[0]),
                                                              Convert.ToSingle(initVelo[1]),
                                                              Convert.ToSingle(initVelo[2]));
                                    Direction.Normalize();
                                    break;
                                case "InitialSpeed":
                                    InitalSpeed = Convert.ToSingle(line[1]);
                                    break;
                                case "DirFromRotation":
                                    if (line[1].Equals("Yes")) DirFromRotation = true;
                                    else DirFromRotation = false;
                                    break;
                                case "MinStartHorizontalVelocity":
                                    MinStartHorizontalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxStartHorizontalVelocity":
                                    MaxStartHorizontalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MinStartVerticalVelocity":
                                    MinStartVerticalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxStartVerticalVelocity":
                                    MaxStartVerticalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MinEndHorizontalVelocity":
                                    MinEndHorizontalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxEndHorizontalVelocity":
                                    MaxEndHorizontalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MinEndVerticalVelocity":
                                    MinEndVerticalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxEndVerticalVelocity":
                                    MaxEndVerticalVelocity = Convert.ToSingle(line[1]);
                                    break;
                                case "MinStartSize":
                                    MinStartSize = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxStartSize":
                                    MaxStartSize = Convert.ToSingle(line[1]);
                                    break;
                                case "MinEndSize":
                                    MinEndSize = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxEndSize":
                                    MaxEndSize = Convert.ToSingle(line[1]);
                                    break;
                                case "MinRotateSpeed":
                                    MinRotateSpeed = Convert.ToSingle(line[1]);
                                    break;
                                case "MaxRotateSpeed":
                                    MaxRotateSpeed = Convert.ToSingle(line[1]);
                                    break;
                                case "MinColor":
                                    string[] mcvect1 = line[1].Split("[, ]".ToCharArray());
                                    MinColor = new Color(Convert.ToInt32(mcvect1[0]),
                                                              Convert.ToInt32(mcvect1[1]),
                                                              Convert.ToInt32(mcvect1[2]),
                                                              Convert.ToInt32(mcvect1[3]));
                                    break;
                                case "MaxColor":
                                    string[] mcvect2 = line[1].Split("[, ]".ToCharArray());
                                    MinColor = new Color(Convert.ToInt32(mcvect2[0]),
                                                              Convert.ToInt32(mcvect2[1]),
                                                              Convert.ToInt32(mcvect2[2]),
                                                              Convert.ToInt32(mcvect2[3]));                                
                                    break;
                                case "SourceBlend": 
                                    string tempLine = line[1].Trim();

                                    foreach (string item in Enum.GetNames(typeof(Blend)))
                                    {
                                        if (tempLine.Equals(item))
                                            SourceBlend = (Blend)Enum.Parse(typeof(Blend), item);
                                    }
                                    break;

                                case "DestinationBlend":                                    
                                    string tempLine2 = line[1].Trim();
                                    foreach (string item in Enum.GetNames(typeof(Blend)))
                                    {
                                        if (tempLine2.Equals(item))
                                            DestinationBlend = (Blend)Enum.Parse(typeof(Blend), item);
                                    }
                                    break;
                            }
                        }
                    }
                }
                //SourceBlend = Blend.SourceAlpha;
                //DestinationBlend = Blend.One;
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

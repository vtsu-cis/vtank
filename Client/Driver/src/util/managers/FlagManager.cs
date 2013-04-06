using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Client.src.service;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Entities;

namespace Client.src.util
{
    public class FlagManager
    {
        private Dictionary<GameSession.Alliance, Flag> flags;

        public FlagManager()
        {
            flags = new Dictionary<GameSession.Alliance, Flag>();
        }

        #region Methods

        public void ShowFlags()
        {
            foreach (Flag flag in flags.Values)
            {
                flag.Hidden = false;
            }
        }

        public void PositionFlag(GameSession.Alliance alliance, Vector3 position)
        {
            if (!flags.ContainsKey(alliance))
            {
                this.AddFlag(alliance, position);
            }

            flags[alliance].ParticleEmitter0.Emitting = true;
            flags[alliance].ParticleEmitter1.Emitting = true;
            flags[alliance].Unattach();
            flags[alliance].Hidden = false;
            flags[alliance].Position = position;
            //TODO stop emitter from flag
        }

        public void FlagPickedUp(GameSession.Alliance alliance, Object3 tank)
        {
            if (!flags.ContainsKey(alliance))
            {
                this.AddFlag(alliance, tank.Position);
            }

            flags[alliance].ParticleEmitter0.Emitting = true;
            flags[alliance].ParticleEmitter1.Emitting = true;
            flags[alliance].MimicPosition(tank, new Vector3(0,0,70));
            flags[alliance].Hidden = false;
            //TODO add continuous particle emitter to flag
            //TODO stop emitter from base
        }

        public void AddFlag(GameSession.Alliance alliance, Vector3 spawnPosition)
        {
            if (!flags.ContainsKey(alliance))
            {
                string a = Enum.GetName(typeof(GameSession.Alliance), alliance);
                a = a.ToLower();
                Model mBase = ServiceManager.Resources.GetModel("events\\" + a + "_flag_spawn");
                Flag nFlag = new Flag(ServiceManager.Resources.GetModel("events\\flag_" + a), spawnPosition);
                //setup emitters
                nFlag.ParticleEmitter0 = new ParticleEmitter(a + "_flag");
                nFlag.ParticleEmitter0.Attach(nFlag, "Emitter0");
                ServiceManager.Scene.Add(nFlag.ParticleEmitter0, 3);
                nFlag.ParticleEmitter1 = new ParticleEmitter(a + "_flag");
                nFlag.ParticleEmitter1.Attach(nFlag, "Emitter1");
                ServiceManager.Scene.Add(nFlag.ParticleEmitter1, 3);
                //end emitters
                flags.Add(alliance, nFlag);
                flags[alliance].RenderID = ServiceManager.Scene.Add(flags[alliance], 0);
                nFlag.BaseRenderID = ServiceManager.Scene.Add(mBase, spawnPosition+ new Vector3(0,0,0.1f), 0);
            }
        }

        public int getFlagId(GameSession.Alliance alliance)
        {
            return flags[alliance].RenderID;
        }

        public void ResetFlag(GameSession.Alliance alliance)
        {
            if (!flags.ContainsKey(alliance))
            {
                this.AddFlag(alliance, Vector3.Zero);
            }

            flags[alliance].ParticleEmitter0.Emitting = true;
            flags[alliance].ParticleEmitter1.Emitting = true;
            flags[alliance].Hidden = false;
            flags[alliance].Unattach();
            flags[alliance].Position = flags[alliance].SpawnPosition;
            //Todo, start continuous emitter from base
        }

        public void SpawnFlag(GameSession.Alliance alliance)
        {
            if (!flags.ContainsKey(alliance))
            {
                this.AddFlag(alliance, Vector3.Zero);
            }

            flags[alliance].ParticleEmitter0.Emitting = true;
            flags[alliance].ParticleEmitter1.Emitting = true;
            flags[alliance].Unattach();
            ResetFlag(alliance);
            flags[alliance].Hidden = false;
            //Todo, start continuous emitter from base
        }

        public void DespawnAll()
        {
            foreach (GameSession.Alliance alliance in flags.Keys)
            {
                DespawnFlag(alliance);
            }
        }

        public void DespawnFlag(GameSession.Alliance alliance)
        {
            if (!flags.ContainsKey(alliance))
            {
                this.AddFlag(alliance, Vector3.Zero);
            }

            flags[alliance].Unattach();
            flags[alliance].ParticleEmitter0.Emitting = false;
            flags[alliance].ParticleEmitter1.Emitting = false;
            flags[alliance].Hidden = true;
        }

        #endregion


        public IEnumerator<KeyValuePair<GameSession.Alliance,Flag>> GetEnumerator()
        {
            return flags.GetEnumerator();
        }

    }
}

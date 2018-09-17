using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Audio
{
    /// <summary>
    /// Collection of sound data
    /// </summary>
    class SoundCollection
    {
        public List<Cue> cueList;
        public List<AudioCategory> categories;

        public SoundCollection()
        {
            Initialize();
        }

        public void Initialize()
        {
            cueList = new List<Cue>();
            categories = new List<AudioCategory>();
        }

        public void AddCategory(String _category)
        {
        }

        public void AddSound(String _sound)
        {
        }

        public void RemoveCategory(String _category)
        {
        }

        public void RemoveSound(String _sound)
        {
        }

        public void Update(GameTime gameTime)
        {
        }

        
    }
}

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace PewPewLazers
{
    public class AudioLibrary
    {
        private SoundEffect fireBullet;
        private Song level1Music;
        private Song startMusic;

        public SoundEffect FireBullet
        {
            get { return fireBullet; }
        }

        public Song StartMusic
        {
            get { return startMusic; }
        }

        public Song Level1Music
        {
            get { return level1Music; }
        }

        public void LoadContent(ContentManager Content)
        {
            fireBullet = Content.Load<SoundEffect>("Sound\\laser");
            startMusic = Content.Load<Song>("Sound\\startMusic");
            level1Music = Content.Load<Song>("Sound\\level1Music");
        }
    }
}
